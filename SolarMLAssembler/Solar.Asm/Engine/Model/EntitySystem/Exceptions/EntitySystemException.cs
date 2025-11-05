namespace Solar.EntitySystem.Exceptions
{
    public class EntitySystemException : Exception
    {
        public EntitySystemException()
        {
        }

        public EntitySystemException(string message) : base(message)
        {
        }

        public EntitySystemException(string? message, Exception inner) : base(message, inner)
        {
        }
    }
}
