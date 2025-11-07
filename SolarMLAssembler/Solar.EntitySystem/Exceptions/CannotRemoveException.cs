namespace Solar.EntitySystem.Exceptions
{
    public class CannotRemoveException : EntitySystemException
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
