using Solar.EntitySystem;

using Solar.Asm.Engine.Model.Exceptions;

namespace Solar.Asm.Engine.Model.Expressions
{
    /// <summary>
    /// Represents a ternary operator expression that takes three values of types <typeparamref name="T1"/>, <typeparamref name="T2"/>, and <typeparamref name="T3"/>
    /// and returns a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public sealed class TernaryExpr<T1, T2, T3, TResult> : Expression<TResult>
        where T1 : IEquatable<T1>
        where T2 : IEquatable<T2>
        where T3 : IEquatable<T3>
        where TResult : IEquatable<TResult>
    {
        public override bool IsConstantExpression =>
            Expr1.Ref!.IsConstantExpression && 
            Expr2.Ref!.IsConstantExpression &&
            Expr3.Ref!.IsConstantExpression;

        public readonly Func<T1, T2, T3, TResult> OperatorFunc;
        public readonly EntityHandle<Expression<T1>> Expr1;
        public readonly EntityHandle<Expression<T2>> Expr2;
        public readonly EntityHandle<Expression<T3>> Expr3;

        // Private constructor to enforce use of From factory method
        private TernaryExpr(
            Expression<T1> expression1,
            Expression<T2> expression2,
            Expression<T3> expression3,
            Func<T1, T2, T3, TResult> operatorFunction)
        {
            Expr1 = expression1.GetHandle();
            Expr2 = expression2.GetHandle();
            Expr3 = expression3.GetHandle();
            OperatorFunc = operatorFunction;
        }

        /// <summary>
        /// Creates a new TernaryExpr from three operand expressions and a ternary operator function.
        /// </summary>
        /// <param name="context">The <see cref="Program"/> this literal expression should exist within</param>
        /// <param name="expression1">The first operand <see cref="Expression{T1}"/></param>
        /// <param name="expression2">The second operand <see cref="Expression{T2}"/></param>
        /// <param name="expression3">The third operand <see cref="Expression{T3}"/></param>
        /// <param name="operatorFunction">The ternary operator function to use</param>
        public static TernaryExpr<T1, T2, T3, TResult> From(
            Program context, 
            Expression<T1> expression1, 
            Expression<T2> expression2, 
            Expression<T3> expression3, 
            Func<T1, T2, T3, TResult> operatorFunction)
        {
            TernaryExpr<T1, T2, T3, TResult> newExpr = new(expression1, expression2, expression3, operatorFunction);
            newExpr.Initialise(context.Expressions);
            return newExpr;
        }

        public override ExpressionResult<TResult> Evaluate()
        {
            GuardValidity();

            ExpressionResult<T1> result1 = Expr1.Ref!.Evaluate();
            ExpressionResult<T2> result2 = Expr2.Ref!.Evaluate();
            ExpressionResult<T3> result3 = Expr3.Ref!.Evaluate();

            if (!result1.HasValue || !result2.HasValue || !result3.HasValue)
                return new ExpressionResult<TResult> { HasValue = false };

            return new ExpressionResult<TResult>
            {
                HasValue = true,
                Value = OperatorFunc(result1.Value, result2.Value, result3.Value)
            };
        }

        public override void Simplify()
        {
            GuardValidity();

            Expr1.Ref!.Simplify();
            Expr2.Ref!.Simplify();
            Expr3.Ref!.Simplify();

            // If the source expression is not constant, we cannot simplify
            if (!Expr1.Ref!.IsConstantExpression || !Expr2.Ref!.IsConstantExpression || !Expr3.Ref!.IsConstantExpression)
                return;

            ExpressionResult<T1> result1 = Expr1.Ref!.Evaluate();
            ExpressionResult<T2> result2 = Expr2.Ref!.Evaluate();
            ExpressionResult<T3> result3 = Expr3.Ref!.Evaluate();

            if (!result1.HasValue || !result2.HasValue || !result3.HasValue)
                throw new SmlaExpressionHasNoValueException("Could not simplify ternary expression because one or more constant operand has no value.", this);

            // Replace this expression with a literal expression of the result value
            this.ReplaceWith(
                LiteralExpr<TResult>.FromValue(
                    OwningProgram,
                    OperatorFunc(result1.Value, result2.Value, result3.Value)
                )
            );
        }

        protected override void OnInvalidated()
        {
            Expr1.Dispose();
            Expr2.Dispose();
            Expr3.Dispose();
        }
    }
}
