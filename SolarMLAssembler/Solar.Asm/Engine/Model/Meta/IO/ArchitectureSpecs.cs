namespace Solar.Asm.Engine.Model.Meta.IO
{
    public enum Endianness { BigEndian, LittleEndian }

    /// <summary>
    /// This record struct indicates the word, address and memory cell sizes an architecture
    /// works with, as well as endianness<br/><br/>
    /// 
    /// <b>Memory Cell:</b> the unit of memory which an address addresses<br/>
    /// <b>Word:</b> The number of cells the architecture can work on at once<br/>
    /// <b>Address:</b> The size of an address in the architecture<br/>
    /// <b>Endianness:</b> The ordering of cells for multi-cell values<br/>
    /// <b>Cell Endianness:</b> The endianness of bytes for a multi-byte cell<br/>
    /// </summary>
    public readonly record struct ArchitectureSpecs
    {
        public readonly ulong MemoryCellMask => (0x1UL << 8 * MemoryCellSizeInBytes) - 1;
        public readonly ulong WordMask => (0x1UL << 8 * WordSizeInCells * MemoryCellSizeInBytes) - 1;
        public readonly ulong AddressMask => (0x1UL << 8 * AddressSizeInCells * MemoryCellSizeInBytes) - 1;

        public readonly string ArchitectureIdCode;
        public readonly byte MemoryCellSizeInBytes;
        public readonly byte WordSizeInCells;
        public readonly byte AddressSizeInCells;
        public readonly Endianness Endianess;
        public readonly Endianness InternalEndianness;

        public ArchitectureSpecs(
            string archIdCode,
            byte wordSizeInCells = 1,
            byte addrSizeInCells = 1,
            byte memoryCellInBytes = 1,
            Endianness endianness = Endianness.LittleEndian,
            Endianness internalEndianness = Endianness.BigEndian
        )
        {
            // Verify valid memory cell size (between 1 and 8 bytes)
            switch (memoryCellInBytes)
            {
                case >= 1 and <= 8:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(memoryCellInBytes), "This assembler only supports cell sizes between 1 and 8 bytes in size");
            }

            // Verify word size (between 1 and 8 bytes)
            switch (memoryCellInBytes * wordSizeInCells)
            {
                case >= 1 and <= 8:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(wordSizeInCells), "This assembler only supports word sizes between 1 and 8 bytes in size");
            }

            // Verify word size (between 1 and 8 bytes)
            switch (memoryCellInBytes * wordSizeInCells)
            {
                case >= 1 and <= 8:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(wordSizeInCells), "This assembler only supports word sizes between 1 and 8 bytes in size");
            }

            // Verify address size (between 1 and 8 bytes)
            switch (memoryCellInBytes * addrSizeInCells)
            {
                case >= 1 and <= 8:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(addrSizeInCells), "This assembler only supports address sizes between 1 and 8 bytes in size");
            }

            ArchitectureIdCode = archIdCode;
            MemoryCellSizeInBytes = memoryCellInBytes;
            WordSizeInCells = wordSizeInCells;
            AddressSizeInCells = addrSizeInCells;
            Endianess = endianness;
            InternalEndianness = internalEndianness;
        }

        public readonly ulong CellFromBytes(byte[] bytes)
        {
            if (bytes.Length != MemoryCellSizeInBytes)
                throw new ArgumentOutOfRangeException(nameof(bytes), $"Received byte array of length {bytes.Length}, expected {MemoryCellSizeInBytes}");

            // Special case that doesn't require looping
            if (MemoryCellSizeInBytes == 1)
                return bytes[0];

            // If cells are internally little-endian, flip them
            if (InternalEndianness == Endianness.LittleEndian)
                bytes = [.. bytes.Reverse()];

            // Generic case
            ulong cell = bytes[0];
            for (int i = 1; i < MemoryCellSizeInBytes; i++)
            {
                cell <<= 8;
                cell |= bytes[i];
            }
            return cell;
        }

        public readonly byte[] CellToBytes(ulong cell)
        {
            // Special case that doesn't require loop
            if (MemoryCellSizeInBytes == 1)
                return [(byte)cell];

            // Convert the cell to bytes in little-endian order
            byte[] bytes = new byte[MemoryCellSizeInBytes];
            for (int i = 0; i < MemoryCellSizeInBytes; i++)
            {
                bytes[i] = (byte)cell;
                cell >>= 8;
            }

            // If cells are internally big endian, flip them
            if (InternalEndianness == Endianness.BigEndian)
                bytes = [.. bytes.Reverse()];

            return bytes;
        }

        public readonly ulong ValueFromNCellBytes(byte[] bytes)
        {
            if (bytes.Length % MemoryCellSizeInBytes != 0)
                throw new ArgumentException($"Byte representation array must be a multiple of the cell size ({MemoryCellSizeInBytes})", nameof(bytes));

            // Number of cells cannot give us more than 8 bytes since ulong is 8 bytes
            switch (bytes.Length)
            {
                case >= 1 and <= 8:
                    break;
                default:
                    throw new ArgumentException("Byte representation array must be 1 to 8 bytes", nameof(bytes) );
            }

            // Chunk cells and convert them to values
            ulong[] cells = [.. bytes.Chunk(MemoryCellSizeInBytes).Select(CellFromBytes)];

            // Special case
            if (cells.Length == 1)
                return cells[0];

            // If little-endian, flip the cells
            if (Endianess == Endianness.LittleEndian)
                cells = [.. cells.Reverse()];

            // Rebuild the ulong
            ulong result = cells[0];
            for (int i = 1; i < cells.Length; i++)
            {
                result <<= 8 * MemoryCellSizeInBytes;
                result |= cells[i];
            }
            return result;
        }

        public readonly byte[] ValueToNCellBytes(ulong value, uint numOfCells)
        {
            // number of cells cannot give us more than 8 bytes since ulong is 8 bytes
            switch (numOfCells * MemoryCellSizeInBytes)
            {
                case >= 1 and <= 8:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(numOfCells), "Number of cells must give a byte representation that is 1 to 8 bytes");
            }

            // Special case for 1 cell
            if (numOfCells == 1)
                return CellToBytes(value);

            // Convert the value to cells in little-endian order
            ulong[] cells = new ulong[numOfCells];
            for (int i = 0; i < numOfCells; i++)
            {
                cells[i] = value & MemoryCellMask;
                value >>= 8 * MemoryCellSizeInBytes;
            }

            // If big-endian, flip them
            if (Endianess == Endianness.BigEndian)
                cells = [.. cells.Reverse()];

            // Convert and flatten the cells to bytes
            return [.. cells.SelectMany(CellToBytes)];
        }

        public readonly ulong WordFromBytes(byte[] bytes)
        {
            if (bytes.Length != MemoryCellSizeInBytes * WordSizeInCells)
                throw new ArgumentException($"Received byte array of length {bytes.Length}, expected {MemoryCellSizeInBytes * WordSizeInCells}", nameof(bytes));

            return ValueFromNCellBytes(bytes);
        }

        public readonly byte[] WordToBytes(ulong word) => ValueToNCellBytes(word, WordSizeInCells);

        public readonly ulong AddressFromBytes(byte[] bytes)
        {

            if (bytes.Length != MemoryCellSizeInBytes * AddressSizeInCells)
                throw new ArgumentException($"Received byte array of length {bytes.Length}, expected {MemoryCellSizeInBytes * AddressSizeInCells}", nameof(bytes));

            return ValueFromNCellBytes(bytes);
        }

        public readonly byte[] AddressToBytes(ulong address) => ValueToNCellBytes(address, AddressSizeInCells);
    }
}
