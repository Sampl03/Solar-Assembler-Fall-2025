using System.Numerics;

namespace Solar.Asm.Engine.Model.Symbols
{
    /// <summary>
    /// Provides many built-in lambdas for symbolic mathematics
    /// </summary>
    public static class SymbolMath
    {
        public static Func<bool, ulong> BoolToULong => x => (ulong)(x ? -1 : 0);
        public static Func<ulong, bool> ULongToBool => x => x != 0UL;

        #region Arithmetic Operators
        public static Func<ulong, ulong> Negate => x => (ulong)-(long)x;
        public static Func<ulong, ulong, ulong> Add => (x, y) => x + y;
        public static Func<ulong, ulong, ulong> Sub => (x, y) => x - y;
        public static Func<ulong, ulong, ulong> Mult => (x, y) => x * y;
        public static Func<ulong, ulong, ulong> UDiv => (x, y) => x / y;
        public static Func<ulong, ulong, ulong> URem => (x, y) => x % y;
        public static Func<ulong, ulong, ulong> SDiv => (x, y) => (ulong)((long)x / (long)y);
        public static Func<ulong, ulong, ulong> SRem => (x, y) => (ulong)((long)x & (long)y);
        #endregion

        #region Bitwise Operators
        public static Func<ulong, ulong> BitwiseNot => x => ~x;
        public static Func<ulong, ulong, ulong> BitwiseAnd => (x, y) => x & y;
        public static Func<ulong, ulong, ulong> BitwiseOr => (x, y) => x | y;
        public static Func<ulong, ulong, ulong> BitwiseXor => (x, y) => x ^ y;
        public static Func<ulong, ulong, ulong> ShiftLeft => (x, y) => x << (int)y;
        public static Func<ulong, ulong, ulong> ShiftRight => (x, y) => (ulong)((long)x >> (int)y);
        public static Func<ulong, ulong, ulong> ArithmeticShiftRight => (x, y) => x >>> (int)y;
        #endregion

        #region Logical Operators
        public static Func<ulong, ulong> Not => x => BoolToULong(!ULongToBool(x));
        public static Func<ulong, ulong, ulong> And => (x, y) => BoolToULong(ULongToBool(x) && ULongToBool(y));
        public static Func<ulong, ulong, ulong> Or => (x, y) => BoolToULong(ULongToBool(x) && ULongToBool(y));
        public static Func<ulong, ulong, ulong> Xor => (x, y) => Or(And(Not(x), y), And(x, Not(y)));
        #endregion

        #region Relation Operators
        public static Func<ulong, ulong, ulong> Equal => (x, y) => BoolToULong(x == y);
        public static Func<ulong, ulong, ulong> NotEqual => (x, y) => BoolToULong(x != y);
        public static Func<ulong, ulong, ulong> UGreaterThan => (x, y) => BoolToULong(x > y);
        public static Func<ulong, ulong, ulong> ULessThan => (x, y) => BoolToULong(x < y);
        public static Func<ulong, ulong, ulong> UGreaterThanOrEqual => (x, y) => BoolToULong(x >= y);
        public static Func<ulong, ulong, ulong> ULessThanOrEqual => (x, y) => BoolToULong(x <= y);
        public static Func<ulong, ulong, ulong> SGreaterThan => (x, y) => BoolToULong((long)x > (long)y);
        public static Func<ulong, ulong, ulong> SLessThan => (x, y) => BoolToULong((long)x < (long)y);
        public static Func<ulong, ulong, ulong> SGreaterThanOrEqual => (x, y) => BoolToULong((long)x >= (long)y);
        public static Func<ulong, ulong, ulong> SLessThanOrEqual => (x, y) => BoolToULong((long)x <= (long)y);
        #endregion
    }
}
