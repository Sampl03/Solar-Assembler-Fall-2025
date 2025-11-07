namespace Solar.EntitySystem.Exceptions
{
    public class CannotMergeException : EntitySystemException
    {
        public CannotMergeException()
        {
        }

        public CannotMergeException(string message) : base(message)
        {
        }

        public CannotMergeException(string? message, Exception inner) : base(message, inner)
        {
        }
    }
}
