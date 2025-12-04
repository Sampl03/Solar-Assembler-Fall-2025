using Solar.EntitySystem;
using Solar.EntitySystem.Behavior;

namespace Solar.Asm.Engine.Model.Expressions
{
    /// <summary>
    /// Represents an expression that returns a constant literal
    /// </summary>
    /// <remarks>
    /// <typeparamref name="TReturn"/> must be a value type that is convertible to other base types and supports equality comparison
    /// </remarks>
    /// <typeparam name="TReturn"></typeparam>
    public sealed class LiteralExpr<TReturn>
        : Expression<TReturn>, IIrreplaceableEntity, IUniqueEntity
        where TReturn : struct, IConvertible, IEquatable<TReturn>
    {
        public override bool IsConstantExpression => true;

        public TReturn Value { get; init; }

        // Private constructor to enforce use of FromValue factory method
        private LiteralExpr(TReturn value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates or retrieves a literal expression instance for the given value
        /// </summary>
        /// <param name="context">The <see cref="Program"/> this literal expression should exist within</param>
        /// <param name="value"></param>
        /// <returns>
        /// The existing or newly created and initialised <see cref="LiteralExpr{TReturn}"/> instance
        /// </returns>
        public static LiteralExpr<TReturn> FromValue(Program context, TReturn value)
        {
            // Check for an existing literal expression with the same value
            LiteralExpr<TReturn>? existingExpr = context.Expressions.SearchEntities<LiteralExpr<TReturn>>().FirstOrDefault(expr => expr.Value.Equals(value));

            if (existingExpr != null)
                return existingExpr;

            // Otherwise create a new one
            LiteralExpr<TReturn> newExpr = new(value);
            newExpr.Initialise(context.Expressions);

            return newExpr;
        }

        public override ExpressionResult<TReturn> Evaluate(Program context, bool resultIsNecessary = false)
        {
            return new() { HasValue = true, Value = Value };
        }

        public override void Simplify(Program context)
        {
            // Already simplified
            return;
        }

        public bool EntityEquivalent(ModelEntity other)
        {
            if (other is LiteralExpr<TReturn> litExprOther)
                return Value.Equals(litExprOther.Value);

            return false;
        }

        public int EntityHash() => Value.GetHashCode();
    }
}
