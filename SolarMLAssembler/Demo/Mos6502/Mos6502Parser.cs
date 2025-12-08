using Solar.Asm.Engine.Model;
using Solar.Asm.Engine.Model.Code;
using Solar.Asm.Engine.Model.Exceptions;
using Solar.Asm.Engine.Model.Expressions;
using Solar.Asm.Engine.Model.IO;

namespace Demo.Mos6502
{
    public sealed class Mos6502Parser : IAssemblyParser
    {
        public static readonly ArchitectureSpecs StaticSpecs = new(
            archIdCode: "Mos6502",
            wordSizeInCells: 1,
            addrSizeInCells: 2,
            memoryCellInBytes: 1,
            endianness: Endianness.LittleEndian
        );

        public ArchitectureSpecs ArchSpecs => StaticSpecs;

        public Chunk ParseChunk(Program context, string source, ExpressionBase[] embeddedExpressions)
        {
            // TODO: all of parsing...
            throw new NotImplementedException();
        }

        public byte[] PatchBytes(ulong currentVirtualAddress, byte[] data, BinaryPatch[] patches)
        {
            var patched_data = (byte[])data.Clone();

            // Iterate over the patches
            foreach (var patch in patches)
            {
                // Skip valueless patches
                var patchValueResult = patch.ValueExpr.Evaluate();
                if (!patchValueResult.HasValue)
                    continue;

                switch (patch.PatchTypeID)
                {
                    case PatchCodes.Word:
                        if (patchValueResult.Value >= 0x100UL)
                            throw new InvalidOperandException($"Mos6502 1-byte mode expected an 8-bit value, got value 0x{patchValueResult.Value:X}");

                        patched_data[patch.CellOffset] = (byte)patchValueResult.Value;
                        break;

                    case PatchCodes.Address:
                        if (patchValueResult.Value >= 0x10000UL)
                            throw new InvalidOperandException($"Mos6502 2-byte mode expected a 16-bit value, got value 0x{patchValueResult.Value:X}");

                        patched_data[patch.CellOffset] = (byte)patchValueResult.Value;
                        patched_data[patch.CellOffset + 1] = (byte)(patchValueResult.Value >> 8);
                        break;
                    case PatchCodes.Relative:
                        long offset = (long)(patchValueResult.Value - (currentVirtualAddress + 2));
                        if (offset != (sbyte)offset) // If the offset doesn't fit in an 8-bit offset, error
                            throw new InvalidOperandException($"Mos6502 relative mode expected an 8-bit signed offset, got value {offset}");
                        patched_data[patch.CellOffset] = (byte)offset;
                        break;
                    default:
                        throw new UnrecognizedPatchException($"Unrecognized patch code '{patch.PatchTypeID}'");
                }
            }

            return patched_data;
        }
    }
}
