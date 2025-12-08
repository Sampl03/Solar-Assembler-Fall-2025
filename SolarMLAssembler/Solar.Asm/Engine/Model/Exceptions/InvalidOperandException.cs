
namespace Solar.Asm.Engine.Model.Exceptions
{
    public class InvalidOperandException : SmlaException
    {
        public InvalidOperandException()
        {
        }

        public InvalidOperandException(string message) : base(message)
        {
        }

        public InvalidOperandException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
