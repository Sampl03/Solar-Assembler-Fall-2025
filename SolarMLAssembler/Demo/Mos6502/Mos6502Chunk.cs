using Solar.Asm.Engine.Model;
using Solar.Asm.Engine.Model.Code;
using Solar.Asm.Engine.Model.Exceptions;
using Solar.Asm.Engine.Model.Expressions;
using Solar.EntitySystem;

namespace Demo.Mos6502
{
    internal enum Mos6502ChunkLayout
    {
        Implied, // No value, always use _shortOpcode
        Immediate, // 1-byte value, always use _shortOpcode
        Mixed, // 1 or 2 byte value, depending on the value of the operand, use _shortOpcode or _longOpcode
        Address, // 2 byte value, always use _longOpcode
        Relative // 1-byte PC-relative offset, always use _shortOpcode. Operand is address
    }

    public class Mos6502Chunk : Chunk
    {
        public Expression<ulong>? Operand => _operandHandle?.Ref!; 
        private readonly EntityHandle<Expression<ulong>>? _operandHandle;

        private readonly Mos6502ChunkLayout _layout;
        private readonly byte _shortOpcode; // Used with implied or 1-byte operand
        private readonly byte _longOpcode; // Used with 2-byte operand

        private Mos6502Chunk(Mos6502ChunkLayout layout,
                             byte shortOpcode = 0,
                             byte longOpcode = 0,
                             Expression<ulong>? operandValue = null)
        {
            operandValue?.GuardValidity();

            _layout = layout;
            _shortOpcode = shortOpcode;
            _longOpcode = longOpcode;
            _operandHandle = operandValue?.GetHandle();
        }

        public static Mos6502Chunk CreateImplied(byte opcode)
        {
            return new Mos6502Chunk(Mos6502ChunkLayout.Implied, shortOpcode: opcode);
        }

        public static Mos6502Chunk CreateImmediate(byte opcode, Expression<ulong> operand)
        {
            return new Mos6502Chunk(Mos6502ChunkLayout.Immediate, shortOpcode: opcode, operandValue: operand);
        }

        public static Mos6502Chunk CreateMixed(byte shortOpcode, byte longOpcode, Expression<ulong> operand)
        {
            return new Mos6502Chunk(Mos6502ChunkLayout.Mixed, shortOpcode: shortOpcode, longOpcode: longOpcode, operandValue: operand);
        }
        public static Mos6502Chunk CreateAddress(byte opcode, Expression<ulong> operand)
        {
            return new Mos6502Chunk(Mos6502ChunkLayout.Address, longOpcode: opcode, operandValue: operand);
        }

        public static Mos6502Chunk CreateRelative(byte opcode, Expression<ulong> operand)
        {
            return new Mos6502Chunk(Mos6502ChunkLayout.Relative, shortOpcode: opcode, operandValue: operand);
        }

        public override IReadOnlyList<byte> EmitBytes()
        {
            GuardValidity();

            // Pre-calculate the operand if we have one
            Operand?.Simplify();
            ExpressionResult<ulong>? operandValueResult = null;

            // To avoid potential infinite recursion, we only evaluate if the operand is constant, and patch later
            if (Operand?.IsConstantExpression ?? false)
                operandValueResult = Operand?.Evaluate();
            else
                operandValueResult = new ExpressionResult<ulong> { HasValue = true, Value = OwningProgram.ArchSpecs.AddressMask };

            if (operandValueResult != null && !operandValueResult.HasValue)
                throw new ExpressionHasNoValueException("Mos6502Chunk operand has no value", Operand!);

            switch (_layout)
            {
                case Mos6502ChunkLayout.Implied:
                    return [_shortOpcode];

                case Mos6502ChunkLayout.Relative:
                    return [_shortOpcode, (byte)(operandValueResult!.Value!.Value - (CalculateMemCellVirtualAddress() + 2))];

                case Mos6502ChunkLayout.Immediate:
                    return [_shortOpcode, (byte)operandValueResult!.Value!.Value];

                case Mos6502ChunkLayout.Address:
                    return [_longOpcode, (byte)operandValueResult!.Value!.Value, (byte)(operandValueResult!.Value!.Value >> 8)];

                case Mos6502ChunkLayout.Mixed:
                    ushort value = (ushort)operandValueResult!.Value!.Value;
                    if (value < 0x100)
                        return [_shortOpcode, (byte)value];
                    else
                        return [_longOpcode, (byte)value, (byte)(value >> 8)];
            }

            throw new InvalidChunkException("Invalid Mos6502Chunk state in EmitBytes", this);
        }

        public override BinaryPatch[] EmitPatches()
        {
            GuardValidity();

            // Pre-calculate the operand if we have one
            Operand?.Simplify();
            var operandValueResult = Operand?.Evaluate();

            if (operandValueResult != null && !operandValueResult.HasValue)
                throw new ExpressionHasNoValueException("Mos6502Chunk operand has no value", Operand!);

            switch (_layout)
            {
                case Mos6502ChunkLayout.Implied:
                    return [];

                case Mos6502ChunkLayout.Relative:
                    long offset = (long)(operandValueResult!.Value!.Value - (CalculateMemCellVirtualAddress() + 2));
                    if (offset != (sbyte)offset) // If the offset doesn't fit in an 8-bit offset, error
                        throw new InvalidOperandException($"Mos6502 relative mode expected an 8-bit signed offset, got value {offset}");

                    if (Operand!.IsConstantExpression)
                        return [];

                    return [new(PatchCodes.Relative, 1, _operandHandle!)];

                case Mos6502ChunkLayout.Immediate:
                    if (operandValueResult!.Value.Value >= 0x100UL)
                        throw new InvalidOperandException($"Mos6502 1-byte mode expected an 8-bit value, got value 0x{operandValueResult!.Value.Value:X}");

                    if (Operand!.IsConstantExpression)
                        return [];

                    return [new(PatchCodes.Word, 1, _operandHandle!)];

                case Mos6502ChunkLayout.Address:
                    if (operandValueResult!.Value.Value >= 0x10000UL)
                        throw new InvalidOperandException($"Mos6502 2-byte mode expected a 16-bit value, got value 0x{operandValueResult!.Value.Value:X}");

                    if (Operand!.IsConstantExpression)
                        return [];

                    return [new(PatchCodes.Address, 1, _operandHandle!)];

                case Mos6502ChunkLayout.Mixed:
                    ulong value = operandValueResult!.Value.Value;

                    if (value >= 0x10000L)
                        throw new InvalidOperandException($"Mos6502 mixed mode expected an 8-bit or 16-bit value, got value 0x{operandValueResult!.Value.Value:X}");

                    if (Operand!.IsConstantExpression)
                        return [];

                    if (value < 0x100UL)
                        return [new(PatchCodes.Word, 1, _operandHandle!)];
                    else
                        return [new(PatchCodes.Address, 1, _operandHandle!)];
            }

            throw new InvalidChunkException("Invalid Mos6502Chunk state in EmitPatches", this);
        }
    }
}
