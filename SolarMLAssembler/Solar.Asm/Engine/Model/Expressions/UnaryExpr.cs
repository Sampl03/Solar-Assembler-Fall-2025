using Solar.EntitySystem;

using Solar.Asm.Engine.Model.Exceptions;

namespace Solar.Asm.Engine.Model.Expressions
{
    /// <summary>
    /// Represents a unary operator expression that takes a value of type <typeparamref name="TOperand"/> and returns a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TOperand"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public sealed class UnaryExpr<TOperand, TResult> : Expression<TResult>
        where TOperand : IEquatable<TOperand>
        where TResult : IEquatable<TResult>
    {
        public override bool IsConstantExpression => OperandExpression.IsConstantExpression;

        public readonly Func<TOperand, TResult> OperatorFunc;
        private readonly EntityHandle<Expression<TOperand>> _operandExpression;
        public Expression<TOperand> OperandExpression => _operandExpression.Ref!;

        // Private constructor to enforce use of From factory method
        private UnaryExpr(Expression<TOperand> operandExpression, Func<TOperand, TResult> castFunction)
        {
            _operandExpression = operandExpression.GetHandle();
            OperatorFunc = castFunction;
        }

        /// <summary>
        /// Creates a new UnaryExpr from an operand expression and a unary operator function
        /// </summary>
        /// <param name="context">The <see cref="Program"/> this literal expression should exist within</param>
        /// <param name="operandExpression">The operand <see cref="Expression{TOperand}"/></param>
        /// <param name="operatorFunction">The unary operator function to use</param>
        public static UnaryExpr<TOperand, TResult> From(Program context, Expression<TOperand> operandExpression, Func<TOperand, TResult> operatorFunction)
        {
            operandExpression.GuardValidity();
            UnaryExpr<TOperand, TResult> newExpr = new(operandExpression, operatorFunction);
            newExpr.Initialise(context.Expressions);
            return newExpr;
        }

        public override ExpressionResult<TResult> Evaluate()
        {
            GuardValidity();

            ExpressionResult<TOperand> operandResult = OperandExpression.Evaluate();

            if (!operandResult.HasValue)
                return new() { HasValue = false };

            return new()
            {
                HasValue = true,
                Value = OperatorFunc(operandResult.Value!)
            };
        }

        public override void Simplify()
        {
            GuardValidity();

            OperandExpression.Simplify();

            // If the operand expression is not constant, we cannot simplify
            if (!OperandExpression.IsConstantExpression)
                return;

            ExpressionResult<TOperand> operandResult = OperandExpression.Evaluate();

            if (!operandResult.HasValue)
                throw new SmlaExpressionHasNoValueException("Could not simplify unary expression because constant operand expression has no value.", this);

            // Replace this expression with a literal expression of the result value
            this.ReplaceWith(
                LiteralExpr<TResult>.FromValue(
                    OwningProgram,
                    OperatorFunc(operandResult.Value!)
                )
            );
        }

        protected override void OnInvalidated()
        {
            _operandExpression.Dispose();
        }
    }
}
