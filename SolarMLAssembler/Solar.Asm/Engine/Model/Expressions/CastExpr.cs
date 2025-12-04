using Solar.EntitySystem;

using Solar.Asm.Engine.Model.Exceptions;

namespace Solar.Asm.Engine.Model.Expressions
{
    /// <summary>
    /// Represents a casting expression that converts a value of type <typeparamref name="TFrom"/> to a value of type <typeparamref name="TTo"/>.
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="TTo"></typeparam>
    public sealed class CastExpr<TFrom, TTo> : Expression<TTo>
        where TFrom : IEquatable<TFrom>
        where TTo : IEquatable<TTo>
    {
        public override bool IsConstantExpression => SourceExpression.Ref!.IsConstantExpression;

        public readonly Func<TFrom, TTo> CastFunction;
        public readonly EntityHandle<Expression<TFrom>> SourceExpression;

        // Private constructor to enforce use of FromSourceExpression factory method
        private CastExpr(Expression<TFrom> sourceExpression, Func<TFrom, TTo> castFunction)
        {
            SourceExpression = sourceExpression.GetHandle();
            CastFunction = castFunction;
        }

        /// <summary>
        /// Creates a new CastExpr from a source expression and a casting function.
        /// </summary>
        /// <param name="context">The <see cref="Program"/> this literal expression should exist within</param>
        /// <param name="sourceExpression">The source <see cref="ExpressionBase{TFrom}"/></param>
        /// <param name="castFunction">The casting function to use</param>
        public static CastExpr<TFrom, TTo> FromSourceExpression(Program context, Expression<TFrom> sourceExpression, Func<TFrom, TTo> castFunction)
        {
            CastExpr<TFrom, TTo> newExpr = new(sourceExpression, castFunction);
            newExpr.Initialise(context.Expressions);
            return newExpr;
        }

        public override ExpressionResult<TTo> Evaluate()
        {
            GuardValidity();

            ExpressionResult<TFrom> sourceResult = SourceExpression.Ref!.Evaluate();

            if (!sourceResult.HasValue)
                return new ExpressionResult<TTo> { HasValue = false };

            return new ExpressionResult<TTo>
            {
                HasValue = true,
                Value = CastFunction(sourceResult.Value!)
            };
        }

        public override void Simplify()
        {
            GuardValidity();

            SourceExpression.Ref!.Simplify();

            // If the source expression is not constant, we cannot simplify
            if (!SourceExpression.Ref!.IsConstantExpression)
                return;

            ExpressionResult<TFrom> sourceResult = SourceExpression.Ref!.Evaluate();

            if (!sourceResult.HasValue)
                throw new SmlaExpressionHasNoValueException("Could not simplify cast expression because source expression has no value.", this);

            // Replace this expression with a literal expression of the casted value
            this.ReplaceWith(
                LiteralExpr<TTo>.FromValue(
                    OwningProgram,
                    CastFunction(sourceResult.Value!)
                )
            );
        }

        protected override void OnInvalidated()
        {
            SourceExpression.Dispose();
        }
    }
}
