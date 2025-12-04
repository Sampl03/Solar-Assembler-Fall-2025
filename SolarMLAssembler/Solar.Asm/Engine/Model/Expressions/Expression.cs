using Solar.EntitySystem;

using Solar.Asm.Engine.Model.Exceptions;

namespace Solar.Asm.Engine.Model.Expressions
{
    /// <summary>
    /// Parent class of Expression to allow non-generic handling of expressions
    /// </summary>
    public abstract class ExpressionBase : ModelEntity
    {
        /// <summary>
        /// The Program in which this expression was declared
        /// </summary>
        public Program OwningProgram
        {
            get
            {
                GuardValidity();
                return (Program)OwningTable!.Context;
            }
        }

        /// <summary>The type this expression returns</summary>
        public abstract Type ReturnType { get; }

        /// <summary>Whether or not the expression is constant and can be simplified to its return value</summary>
        public abstract bool IsConstantExpression { get; }
    }

    /// <summary>
    /// Parent class for all expressions that return a value of type <typeparamref name="TReturn"/>
    /// </summary>
    /// <typeparam name="TReturn">The return type of this expression</typeparam>
    public abstract class Expression<TReturn> : ExpressionBase
    {
        public sealed override Type ReturnType => typeof(TReturn);

        /// <summary>
        /// Calculates the value of this expression within its own context, returning an <see cref="ExpressionResult{TReturn}"/>
        /// </summary>
        /// <remarks>
        /// If an expression in this tree requires a result but could not get one, this method may throw a <see cref="SmlaExpressionHasNoValueException"/>
        /// </remarks>
        /// <returns>
        /// An <see cref="ExpressionResult{TReturn}"/> containing the result of the evaluation if there was one
        /// </returns>
        /// <exception cref="SmlaExpressionHasNoValueException"></exception>
        public abstract ExpressionResult<TReturn> Evaluate();

        /// <summary>
        /// Simplifies this expression as much as possible
        /// </summary>
        /// <remarks>
        /// If an expression in this tree reported itself as constant but its value could not be determined, this method may throw a <see cref="SmlaExpressionHasNoValueException"/>"/>
        /// </remarks>
        /// <exception cref="SmlaExpressionHasNoValueException"></exception>
        public abstract void Simplify();
    }
}
