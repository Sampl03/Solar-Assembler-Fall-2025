namespace Solar.Asm.Engine.Model.Exceptions
{
    public class SmlaException : Exception
    {
        public SmlaException()
        {
        }

        public SmlaException(string message) : base(message)
        {
        }

        public SmlaException(string? message, Exception inner) : base(message, inner)
        {
        }
    }
}
