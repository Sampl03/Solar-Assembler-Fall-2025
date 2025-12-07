
namespace Solar.Asm.Engine.Model.Exceptions
{
    public class CannotAddException : SmlaException
    {
        public CannotAddException() { }
        public CannotAddException(string message) : base(message) { }
        public CannotAddException(string message, Exception inner) : base(message, inner) { }
    }
}
