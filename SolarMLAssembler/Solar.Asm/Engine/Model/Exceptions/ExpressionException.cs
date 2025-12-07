
using Solar.Asm.Engine.Model.Expressions;

namespace Solar.Asm.Engine.Model.Exceptions
{
    public class ExpressionException : SmlaException
    {
        public readonly ExpressionBase? Expression;

        public ExpressionException()
        {
        }

        public ExpressionException(string message) : base(message)
        {
        }

        public ExpressionException(string message, Exception inner) : base(message, inner)
        {
        }

        public ExpressionException(string message, ExpressionBase expression) : this(message)
        {
            Expression = expression;
        }

        public ExpressionException(string message, ExpressionBase expression, Exception inner) : this(message, inner)
        {
            Expression = expression;
        }
    }
}
