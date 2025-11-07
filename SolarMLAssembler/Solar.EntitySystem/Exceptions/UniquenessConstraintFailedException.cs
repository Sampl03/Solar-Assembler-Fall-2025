namespace Solar.EntitySystem.Exceptions
{
    public class UniquenessConstraintFailedException : EntitySystemException
    {
        public UniquenessConstraintFailedException()
        {
        }

        public UniquenessConstraintFailedException(string message) : base(message)
        {
        }

        public UniquenessConstraintFailedException(string? message, Exception inner) : base(message, inner)
        {
        }
    }
}
