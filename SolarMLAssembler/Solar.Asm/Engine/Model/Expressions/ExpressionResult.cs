namespace Solar.Asm.Engine.Model.Expressions
{
    public readonly struct ExpressionResult<T>(bool hasValue, T? value)
    {
        public readonly bool HasValue { get; init; } = hasValue;
        public readonly T? Value { get; init; } = value;
    }
}
