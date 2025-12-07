
namespace Solar.Asm.Engine.Model.Exceptions
{
    public class CannotOutputException : SmlaException
    {
        public CannotOutputException()
        {
        }

        public CannotOutputException(string message) : base(message)
        {
        }

        public CannotOutputException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
