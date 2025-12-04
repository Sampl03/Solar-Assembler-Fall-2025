
using Solar.Asm.Engine.Model.Expressions;

namespace Solar.Asm.Engine.Model.Exceptions
{
    public class SmlaExpressionHasNoValueException : SmlaExpressionException
    {
        public readonly ExpressionBase? Expression;

        public SmlaExpressionHasNoValueException()
        {
        }

        public SmlaExpressionHasNoValueException(string message) : base(message)
        {
        }

        public SmlaExpressionHasNoValueException(string message, Exception inner) : base(message, inner)
        {
        }

        public SmlaExpressionHasNoValueException(string message, ExpressionBase expression) : this(message)
        {
            Expression = expression;
        }

        public SmlaExpressionHasNoValueException(string message, ExpressionBase expression, Exception inner) : this(message, inner)
        {
            Expression = expression;
        }
    }
}
