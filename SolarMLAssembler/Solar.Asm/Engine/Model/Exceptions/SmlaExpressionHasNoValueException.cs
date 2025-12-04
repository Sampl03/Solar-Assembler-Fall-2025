
using Solar.Asm.Engine.Model.Expressions;

namespace Solar.Asm.Engine.Model.Exceptions
{
    public class SmlaExpressionHasNoValueException : SmlaExpressionException
    {
        public SmlaExpressionHasNoValueException()
        {
        }

        public SmlaExpressionHasNoValueException(string message) : base(message)
        {
        }

        public SmlaExpressionHasNoValueException(string message, Exception inner) : base(message, inner)
        {
        }

        public SmlaExpressionHasNoValueException(string message, ExpressionBase expression) : base(message, expression)
        {
        }

        public SmlaExpressionHasNoValueException(string message, ExpressionBase expression, Exception inner) : base(message, expression, inner)
        {
        }
    }
}
