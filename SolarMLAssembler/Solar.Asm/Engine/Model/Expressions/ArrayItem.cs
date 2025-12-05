using System.Collections.Immutable;
using Solar.Asm.Engine.Model.Exceptions;
using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Expressions
{
    /// <summary>
    /// Represents the kind of item ArrayItem is
    /// </summary>
    internal enum ArrayItemKind
    {
        Invalid = 0,        // Here to prevent defaulting to valid state
        ConstantSingle,     // baked-in TReturn value
        ConstantSubArray,   // baked-in TReturn[] value
        SingleExpression,   // handle to a single Expression<TReturn>
        SubArray            // handle to another ArrayExpr<TReturn>
    }

    /// <summary>
    /// Internal struct representing an item within an <see cref="ArrayExpr{TReturn}"/>
    /// </summary>
    /// <remarks>
    /// <see cref="Cleanup"/> must be called after use to dispose of any held handles.
    /// </remarks>
    /// <typeparam name="TReturn"></typeparam>
    internal readonly struct ArrayItem<TReturn>
        where TReturn : IEquatable<TReturn>
    {
        public readonly ArrayItemKind Kind;
        public readonly TReturn? ConstantValue;
        public readonly TReturn[]? ConstantSubArray;
        public readonly EntityHandle<Expression<TReturn>>? Expression;
        public readonly EntityHandle<ArrayExpr<TReturn>>? SubArray;

        public ArrayItem(TReturn value)
        {
            Kind = ArrayItemKind.ConstantSingle;
            ConstantValue = value;
        }

        public ArrayItem(IEnumerable<TReturn> values)
        {
            Kind = ArrayItemKind.ConstantSubArray;
            ConstantSubArray = [.. values];
        }

        public ArrayItem(Expression<TReturn> expression)
        {
            Kind = ArrayItemKind.SingleExpression;
            Expression = expression.GetHandle();
        }

        public ArrayItem(ArrayExpr<TReturn> subArray)
        {
            Kind = ArrayItemKind.SubArray;
            SubArray = subArray.GetHandle();
        }

        public bool IsConstant()
        {
            return Kind switch
            {
                ArrayItemKind.ConstantSingle => true,
                ArrayItemKind.ConstantSubArray => true,
                ArrayItemKind.SingleExpression => Expression!.Ref!.IsConstantExpression,
                ArrayItemKind.SubArray => SubArray!.Ref!.IsConstantExpression,
                _ => false,
            };
        }

        public ExpressionResult<IEnumerable<TReturn>> GetValues(in ArrayExpr<TReturn> caller)
        {
            switch (Kind)
            {
                case ArrayItemKind.ConstantSingle:
                    return new() { HasValue = true, Value = [ConstantValue!] };

                case ArrayItemKind.ConstantSubArray:
                    return new() { HasValue = true, Value = ConstantSubArray! };

                case ArrayItemKind.SingleExpression:
                    ExpressionResult<TReturn> exprResult = Expression!.Ref!.Evaluate();

                    if (!exprResult.HasValue)
                        return new() { HasValue = false }; // If any item has no value, the whole array has no value

                    return new() { HasValue = true, Value = [exprResult.Value!] };

                case ArrayItemKind.SubArray:
                    ExpressionResult<ImmutableArray<TReturn>> subarrayResult = SubArray!.Ref!.Evaluate();

                    if (!subarrayResult.HasValue)
                        return new() { HasValue = false }; // If any item has no value, the whole array has no value

                    return new() { HasValue = true, Value = subarrayResult.Value! };
                default:
                    throw new SmlaExpressionException("Invalid ArrayExpr item encountered during evaluation.", caller);
            }
        }

        public void Cleanup()
        {
            Expression?.Dispose();
            SubArray?.Dispose();
        }
    }
}
