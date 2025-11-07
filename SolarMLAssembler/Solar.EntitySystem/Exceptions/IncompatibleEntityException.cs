namespace Solar.EntitySystem.Exceptions
{
    public class IncompatibleEntityException : EntitySystemException
    {
        public Type? EntityTypeExpected { get; init; }
        public Type? EntityTypeReceived { get; init; }

        public IncompatibleEntityException()
        {
        }

        public IncompatibleEntityException(string message) : base(message)
        {
        }

        public IncompatibleEntityException(string? message, Exception inner) : base(message, inner)
        {
        }

        public IncompatibleEntityException(string message, Type expectedType, Type receivedType) : this(message)
        {
            EntityTypeExpected = expectedType;
            EntityTypeReceived = receivedType;
        }

        public IncompatibleEntityException(string? message, Type expectedType, Type receivedType, Exception inner) : this(message, inner)
        {
            EntityTypeExpected = expectedType;
            EntityTypeReceived = receivedType;
        }
    }
}
