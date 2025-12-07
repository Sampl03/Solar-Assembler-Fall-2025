
using Solar.Asm.Engine.Model.Expressions;

namespace Solar.Asm.Engine.Model.Exceptions
{
    public class ExpressionHasNoValueException : ExpressionException
    {
        public ExpressionHasNoValueException()
        {
        }

        public ExpressionHasNoValueException(string message) : base(message)
        {
        }

        public ExpressionHasNoValueException(string message, Exception inner) : base(message, inner)
        {
        }

        public ExpressionHasNoValueException(string message, ExpressionBase expression) : base(message, expression)
        {
        }

        public ExpressionHasNoValueException(string message, ExpressionBase expression, Exception inner) : base(message, expression, inner)
        {
        }
    }
}
