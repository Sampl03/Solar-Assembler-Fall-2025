
using Solar.Asm.Engine.Model.Expressions;

namespace Solar.Asm.Engine.Model.Exceptions
{
    public class SmlaExpressionException : SmlaException
    {
        public readonly ExpressionBase? Expression;

        public SmlaExpressionException()
        {
        }

        public SmlaExpressionException(string message) : base(message)
        {
        }

        public SmlaExpressionException(string message, Exception inner) : base(message, inner)
        {
        }

        public SmlaExpressionException(string message, ExpressionBase expression) : this(message)
        {
            Expression = expression;
        }

        public SmlaExpressionException(string message, ExpressionBase expression, Exception inner) : this(message, inner)
        {
            Expression = expression;
        }
    }
}
