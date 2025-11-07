namespace Solar.EntitySystem.Exceptions
{
    public class ManagerMismatchException : EntitySystemException
    {
        public ManagerMismatchException()
        {
        }

        public ManagerMismatchException(string message) : base(message)
        {
        }

        public ManagerMismatchException(string? message, Exception inner) : base(message, inner)
        {
        }
    }
}
