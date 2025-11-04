
namespace Solar.Asm.Engine.Model.Exceptions
{
    public class CannotRemoveException : SmlaException
    {
        public CannotRemoveException()
        {
        }

        public CannotRemoveException(string message) : base(message)
        {
        }

        public CannotRemoveException(string? message, Exception inner) : base(message, inner)
        {
        }
    }
}
