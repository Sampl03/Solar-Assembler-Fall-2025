namespace Demo.Mos6502
{
    public enum Mos6502AddressMode
    {
        AccImplied, // Accumulator and Implied behave the same
        Absolute, AbsoluteX, AbsoluteY,
        Immediate,
        Indirect, XIndirectZpg, IndirectYZpg,
        Relative,
        ZeroPage, ZeroPageX, ZeroPageY
    }

    public static class Mos6502InstructionTable
    {
        public static byte? TryFindOpcode(string mnemonic, Mos6502AddressMode addressMode)
        {
            var match = InstructionList.Where(x => x.Info.Mnemonic == mnemonic)
                                       .Where(x => x.Info.AddressMode == addressMode)
                                       .Select(x => x.Opcode);

            if (match.Any())
                return match.Single(); // there can only be one!
            return null;
        }

        public static (Mos6502AddressMode AddressMode, byte Opcode)[]? TryFindMnemonicAddressingModes(string mnemonic)
        {
            var match = InstructionList.Where(x => x.Info.Mnemonic == mnemonic)
                                       .Select(x => (x.Info.AddressMode, x.Opcode));

            if (match.Any())
                return [.. match];
            return null;
        }

        public static (string Mnemonic, Mos6502AddressMode AddressMode)? TryFindMnemonic(byte opcode)
        {
            var match = InstructionList.Where(x => x.Opcode == opcode)
                                       .Select(x => x.Info);

            if (match.Any())
                return match.Single(); // there can only be one!
            return null;
        }

        public static readonly ((string Mnemonic, Mos6502AddressMode AddressMode) Info, byte Opcode)[] InstructionList =
        {
            // Transfer
            (("LDA", Mos6502AddressMode.Immediate    ), 0xA9),
            (("LDA", Mos6502AddressMode.ZeroPage     ), 0xA5), (("LDA", Mos6502AddressMode.ZeroPageX    ), 0xB5),
            (("LDA", Mos6502AddressMode.Absolute     ), 0xAD), (("LDA", Mos6502AddressMode.AbsoluteX    ), 0xBD), (("LDA", Mos6502AddressMode.AbsoluteY    ), 0xB9),
            (("LDA", Mos6502AddressMode.XIndirectZpg ), 0xA1), (("LDA", Mos6502AddressMode.IndirectYZpg ), 0xB1),

            (("LDX", Mos6502AddressMode.Immediate    ), 0xA2),
            (("LDX", Mos6502AddressMode.ZeroPage     ), 0xA6), (("LDX", Mos6502AddressMode.ZeroPageY    ), 0xB6),
            (("LDX", Mos6502AddressMode.Absolute     ), 0xAE), (("LDX", Mos6502AddressMode.AbsoluteY    ), 0xBE),

            (("LDY", Mos6502AddressMode.Immediate    ), 0xA0),
            (("LDY", Mos6502AddressMode.ZeroPage     ), 0xA4), (("LDY", Mos6502AddressMode.ZeroPageX    ), 0xB4),
            (("LDY", Mos6502AddressMode.Absolute     ), 0xAC), (("LDY", Mos6502AddressMode.AbsoluteX    ), 0xBC),
            (("STA", Mos6502AddressMode.ZeroPage     ), 0x85), (("STA", Mos6502AddressMode.ZeroPageX    ), 0x95),
            (("STA", Mos6502AddressMode.Absolute     ), 0x8D), (("STA", Mos6502AddressMode.AbsoluteX    ), 0x9D), (("STA", Mos6502AddressMode.AbsoluteY    ), 0x99),
            (("STA", Mos6502AddressMode.XIndirectZpg ), 0x81), (("STA", Mos6502AddressMode.IndirectYZpg ), 0x91),

            (("STX", Mos6502AddressMode.ZeroPage     ), 0x86), (("STX", Mos6502AddressMode.ZeroPageY    ), 0x96),
            (("STX", Mos6502AddressMode.Absolute     ), 0x8E),

            (("STY", Mos6502AddressMode.ZeroPage     ), 0x84), (("STY", Mos6502AddressMode.ZeroPageX    ), 0x94),
            (("STY", Mos6502AddressMode.Absolute     ), 0x8C),

            (("TAX", Mos6502AddressMode.AccImplied   ), 0xAA),
            (("TAY", Mos6502AddressMode.AccImplied   ), 0xA8),
            (("TSX", Mos6502AddressMode.AccImplied   ), 0xBA),
            (("TXA", Mos6502AddressMode.AccImplied   ), 0x8A),
            (("TXS", Mos6502AddressMode.AccImplied   ), 0x9A),
            (("TYA", Mos6502AddressMode.AccImplied   ), 0x98),

            // Stack Instructions
            (("PHA", Mos6502AddressMode.AccImplied   ), 0x48),
            (("PHP", Mos6502AddressMode.AccImplied   ), 0x08),
            (("PLA", Mos6502AddressMode.AccImplied   ), 0x68),
            (("PLP", Mos6502AddressMode.AccImplied   ), 0x28),

            // Decrements & Increments
            (("DEC", Mos6502AddressMode.ZeroPage     ), 0xC6), (("DEC", Mos6502AddressMode.ZeroPageX    ), 0xD6),
            (("DEC", Mos6502AddressMode.Absolute     ), 0xCE), (("DEC", Mos6502AddressMode.AbsoluteX    ), 0xDE),
            (("DEX", Mos6502AddressMode.AccImplied   ), 0xCA),
            (("DEY", Mos6502AddressMode.AccImplied   ), 0x88),

            (("INC", Mos6502AddressMode.ZeroPage     ), 0xE6), (("INC", Mos6502AddressMode.ZeroPageX    ), 0xF6),
            (("INC", Mos6502AddressMode.Absolute     ), 0xEE), (("INC", Mos6502AddressMode.AbsoluteX    ), 0xFE),
            (("INX", Mos6502AddressMode.AccImplied   ), 0xE8),
            (("INY", Mos6502AddressMode.AccImplied   ), 0xC8),

            // Arithmetic Operations
            (("ADC", Mos6502AddressMode.Immediate    ), 0x69),
            (("ADC", Mos6502AddressMode.ZeroPage     ), 0x65), (("ADC", Mos6502AddressMode.ZeroPageX    ), 0x75),
            (("ADC", Mos6502AddressMode.Absolute     ), 0x6D), (("ADC", Mos6502AddressMode.AbsoluteX    ), 0x7D), (("ADC", Mos6502AddressMode.AbsoluteY    ), 0x79),
            (("ADC", Mos6502AddressMode.XIndirectZpg ), 0x61), (("ADC", Mos6502AddressMode.IndirectYZpg ), 0x71),

            (("SBC", Mos6502AddressMode.Immediate    ), 0xE9),
            (("SBC", Mos6502AddressMode.ZeroPage     ), 0xE5), (("SBC", Mos6502AddressMode.ZeroPageX    ), 0xF5),
            (("SBC", Mos6502AddressMode.Absolute     ), 0xED), (("SBC", Mos6502AddressMode.AbsoluteX    ), 0xFD), (("SBC", Mos6502AddressMode.AbsoluteY    ), 0xF9),
            (("SBC", Mos6502AddressMode.XIndirectZpg ), 0xE1), (("SBC", Mos6502AddressMode.IndirectYZpg ), 0xF1),

            // Logical Operations
            (("AND", Mos6502AddressMode.Immediate    ), 0x29),
            (("AND", Mos6502AddressMode.ZeroPage     ), 0x25), (("AND", Mos6502AddressMode.ZeroPageX    ), 0x35),
            (("AND", Mos6502AddressMode.Absolute     ), 0x2D), (("AND", Mos6502AddressMode.AbsoluteX    ), 0x3D), (("AND", Mos6502AddressMode.AbsoluteY    ), 0x39),
            (("AND", Mos6502AddressMode.XIndirectZpg ), 0x21), (("AND", Mos6502AddressMode.IndirectYZpg ), 0x31),

            (("EOR", Mos6502AddressMode.Immediate    ), 0x49),
            (("EOR", Mos6502AddressMode.ZeroPage     ), 0x45), (("EOR", Mos6502AddressMode.ZeroPageX    ), 0x55),
            (("EOR", Mos6502AddressMode.Absolute     ), 0x4D), (("EOR", Mos6502AddressMode.AbsoluteX    ), 0x5D), (("EOR", Mos6502AddressMode.AbsoluteY    ), 0x59),
            (("EOR", Mos6502AddressMode.XIndirectZpg ), 0x41), (("EOR", Mos6502AddressMode.IndirectYZpg ), 0x51),

            (("ORA", Mos6502AddressMode.Immediate    ), 0x09),
            (("ORA", Mos6502AddressMode.ZeroPage     ), 0x05), (("ORA", Mos6502AddressMode.ZeroPageX    ), 0x15),
            (("ORA", Mos6502AddressMode.Absolute     ), 0x0D), (("ORA", Mos6502AddressMode.AbsoluteX    ), 0x1D), (("ORA", Mos6502AddressMode.AbsoluteY    ), 0x19),
            (("ORA", Mos6502AddressMode.XIndirectZpg ), 0x01), (("ORA", Mos6502AddressMode.IndirectYZpg ), 0x11),

            // Shift & Rotate Operations
            (("ASL", Mos6502AddressMode.AccImplied   ), 0x0A),
            (("ASL", Mos6502AddressMode.ZeroPage     ), 0x06), (("ASL", Mos6502AddressMode.ZeroPageX    ), 0x16),
            (("ASL", Mos6502AddressMode.Absolute     ), 0x0E), (("ASL", Mos6502AddressMode.AbsoluteX    ), 0x1E),

            (("LSR", Mos6502AddressMode.AccImplied   ), 0x4A),
            (("LSR", Mos6502AddressMode.ZeroPage     ), 0x46), (("LSR", Mos6502AddressMode.ZeroPageX    ), 0x56),
            (("LSR", Mos6502AddressMode.Absolute     ), 0x4E), (("LSR", Mos6502AddressMode.AbsoluteX    ), 0x5E),

            (("ROL", Mos6502AddressMode.AccImplied   ), 0x2A),
            (("ROL", Mos6502AddressMode.ZeroPage     ), 0x26), (("ROL", Mos6502AddressMode.ZeroPageX    ), 0x36),
            (("ROL", Mos6502AddressMode.Absolute     ), 0x2E), (("ROL", Mos6502AddressMode.AbsoluteX    ), 0x3E),

            (("ROR", Mos6502AddressMode.AccImplied   ), 0x6A),
            (("ROR", Mos6502AddressMode.ZeroPage     ), 0x66), (("ROR", Mos6502AddressMode.ZeroPageX    ), 0x76),
            (("ROR", Mos6502AddressMode.Absolute     ), 0x6E), (("ROR", Mos6502AddressMode.AbsoluteX    ), 0x7E),

            // Flag Instructions
            (("CLC", Mos6502AddressMode.AccImplied   ), 0x18),
            (("CLD", Mos6502AddressMode.AccImplied   ), 0xD8),
            (("CLI", Mos6502AddressMode.AccImplied   ), 0x58),
            (("CLV", Mos6502AddressMode.AccImplied   ), 0xB8),
            (("SEC", Mos6502AddressMode.AccImplied   ), 0x38),
            (("SED", Mos6502AddressMode.AccImplied   ), 0xF8),
            (("SEI", Mos6502AddressMode.AccImplied   ), 0x78),

            // Comparisons
            (("CMP", Mos6502AddressMode.Immediate    ), 0xC9),
            (("CMP", Mos6502AddressMode.ZeroPage     ), 0xC5), (("CMP", Mos6502AddressMode.ZeroPageX    ), 0xD5),
            (("CMP", Mos6502AddressMode.Absolute     ), 0xCD), (("CMP", Mos6502AddressMode.AbsoluteX    ), 0xDD), (("CMP", Mos6502AddressMode.AbsoluteY    ), 0xD9),
            (("CMP", Mos6502AddressMode.XIndirectZpg ), 0xC1), (("CMP", Mos6502AddressMode.IndirectYZpg ), 0xD1),

            (("CPX", Mos6502AddressMode.Immediate    ), 0xE0),
            (("CPX", Mos6502AddressMode.ZeroPage     ), 0xE4),
            (("CPX", Mos6502AddressMode.Absolute     ), 0xEC),

            (("CPY", Mos6502AddressMode.Immediate    ), 0xC0),
            (("CPY", Mos6502AddressMode.ZeroPage     ), 0xC4),
            (("CPY", Mos6502AddressMode.Absolute     ), 0xCC),

            // Bit Test
            (("BIT", Mos6502AddressMode.ZeroPage     ), 0x24),
            (("BIT", Mos6502AddressMode.Absolute     ), 0x2C),

            // Conditional Branch Instructions
            (("BCC", Mos6502AddressMode.Relative     ), 0x90),
            (("BCS", Mos6502AddressMode.Relative     ), 0xB0),
            (("BEQ", Mos6502AddressMode.Relative     ), 0xF0),
            (("BMI", Mos6502AddressMode.Relative     ), 0x30),
            (("BNE", Mos6502AddressMode.Relative     ), 0xD0),
            (("BPL", Mos6502AddressMode.Relative     ), 0x10),
            (("BVC", Mos6502AddressMode.Relative     ), 0x50),
            (("BVS", Mos6502AddressMode.Relative     ), 0x70),

            // Jumps & Subroutines
            (("JMP", Mos6502AddressMode.Absolute     ), 0x4C),
            (("JMP", Mos6502AddressMode.Indirect     ), 0x6C),
            (("JSR", Mos6502AddressMode.Absolute     ), 0x20),
            (("RTS", Mos6502AddressMode.AccImplied   ), 0x60),

            // Interrupts
            (("BRK", Mos6502AddressMode.AccImplied   ), 0x00),
            (("RTI", Mos6502AddressMode.AccImplied   ), 0x40),

            // Other
            (("NOP", Mos6502AddressMode.AccImplied   ), 0xEA),
        };
    }
}
