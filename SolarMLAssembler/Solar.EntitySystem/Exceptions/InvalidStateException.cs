namespace Solar.EntitySystem.Exceptions
{
    public class InvalidStateException : EntitySystemException
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
