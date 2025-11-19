
namespace Solar.Asm.Engine.Model.Exceptions
{
    public class SmlaCannotAddException : SmlaException
    {
        public SmlaCannotAddException() { }
        public SmlaCannotAddException(string message) : base(message) { }
        public SmlaCannotAddException(string message, Exception inner) : base(message, inner) { }
    }
}
