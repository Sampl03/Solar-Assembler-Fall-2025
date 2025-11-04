
namespace Solar.Asm.Engine.Model.Exceptions
{
    public class InvalidStateException : SmlaException
    {
        public InvalidStateException()
        {
        }

        public InvalidStateException(string message) : base(message)
        {
        }

        public InvalidStateException(string? message, Exception inner) : base(message, inner)
        {
        }
    }
}
