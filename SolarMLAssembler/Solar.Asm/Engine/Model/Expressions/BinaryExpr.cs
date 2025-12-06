using Solar.EntitySystem;

using Solar.Asm.Engine.Model.Exceptions;

namespace Solar.Asm.Engine.Model.Expressions
{
    /// <summary>
    /// Represents a binary operator expression that takes a pair of values of types <typeparamref name="TLeft"/> and <typeparamref name="TRight"/>
    /// and returns a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TLeft"></typeparam>
    /// <typeparam name="TRight"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public sealed class BinaryExpr<TLeft, TRight, TResult> : Expression<TResult>
        where TLeft : IEquatable<TLeft>
        where TRight : IEquatable<TRight>
        where TResult : IEquatable<TResult>
    {
        public override bool IsConstantExpression => LeftExpression.IsConstantExpression && RightExpression.IsConstantExpression;

        public readonly Func<TLeft, TRight, TResult> OperatorFunc;
        private readonly EntityHandle<Expression<TLeft>> _leftExpr;
        private readonly EntityHandle<Expression<TRight>> _rightExpr;
        public Expression<TLeft> LeftExpression => _leftExpr.Ref!;
        public Expression<TRight> RightExpression => _rightExpr.Ref!;


        // Private constructor to enforce use of From factory method
        private BinaryExpr(Expression<TLeft> leftExpression, Expression<TRight> rightExpression, Func<TLeft, TRight, TResult> operatorFunction)
        {
            _leftExpr = leftExpression.GetHandle();
            _rightExpr = rightExpression.GetHandle();
            OperatorFunc = operatorFunction;
        }

        /// <summary>
        /// Creates a new BinaryExpr from two operand expressions and a binary operator function.
        /// </summary>
        /// <param name="context">The <see cref="Program"/> this literal expression should exist within</param>
        /// <param name="leftExpression">The left operand <see cref="Expression{TLeft}"/></param>
        /// <param name="rightExpression">The right operand <see cref="Expression{TRight}"/></param>
        /// <param name="operatorFunction">The binary operator function to use</param>
        public static BinaryExpr<TLeft, TRight, TResult> From(
            Program context, 
            Expression<TLeft> leftExpression, 
            Expression<TRight> rightExpression,
            Func<TLeft, TRight, TResult> operatorFunction)
        {
            leftExpression.GuardValidity();
            rightExpression.GuardValidity();
            BinaryExpr<TLeft, TRight, TResult> newExpr = new(leftExpression, rightExpression, operatorFunction);
            newExpr.Initialise(context.Expressions);
            return newExpr;
        }

        public override ExpressionResult<TResult> Evaluate()
        {
            GuardValidity();

            ExpressionResult<TLeft> leftResult = LeftExpression.Evaluate();
            ExpressionResult<TRight> rightResult = RightExpression.Evaluate();

            if (!leftResult.HasValue || !rightResult.HasValue)
                return new() { HasValue = false };

            return new()
            {
                HasValue = true,
                Value = OperatorFunc(leftResult.Value!, rightResult.Value!)
            };
        }

        public override void Simplify()
        {
            GuardValidity();

            LeftExpression.Simplify();
            RightExpression.Simplify();

            // If the source expression is not constant, we cannot simplify
            if (!LeftExpression.IsConstantExpression || !RightExpression.IsConstantExpression)
                return;

            ExpressionResult<TLeft> leftResult = LeftExpression.Evaluate();
            ExpressionResult<TRight> rightResult = RightExpression.Evaluate();

            if (!leftResult.HasValue || !rightResult.HasValue)
                throw new SmlaExpressionHasNoValueException("Could not simplify binary expression because one or more constant operand has no value.", this);

            // Replace this expression with a literal expression of the result value
            this.ReplaceWith(
                LiteralExpr<TResult>.FromValue(
                    OwningProgram,
                    OperatorFunc(leftResult.Value!, rightResult.Value!)
                )
            );
        }

        protected override void OnInvalidated()
        {
            _leftExpr.Dispose();
            _rightExpr.Dispose();
        }
    }
}
