
namespace Solar.Asm.Engine.Model.Exceptions
{
    public class SmlaExpressionException : SmlaException
    {
        public SmlaExpressionException()
        {
        }

        public SmlaExpressionException(string message) : base(message)
        {
        }

        public SmlaExpressionException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
