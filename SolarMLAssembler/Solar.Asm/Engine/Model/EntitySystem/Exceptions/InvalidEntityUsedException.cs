namespace Solar.EntitySystem.Exceptions
{
    public class InvalidEntityUsedException : EntitySystemException
    {
        public InvalidEntityUsedException()
        {
        }

        public InvalidEntityUsedException(string message) : base(message)
        {
        }

        public InvalidEntityUsedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
