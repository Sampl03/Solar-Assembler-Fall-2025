
namespace Solar.Asm.Engine.Model.Exceptions
{
    public class IncompatibleEntityException : SmlaException
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

        public IncompatibleEntityException(string message, Type expectedType, Type receivedType) : base(message)
        {
            EntityTypeExpected = expectedType;
            EntityTypeReceived = receivedType;
        }

        public IncompatibleEntityException(string? message, Type expectedType, Type receivedType, Exception inner) : base(message, inner)
        {
            EntityTypeExpected = expectedType;
            EntityTypeReceived = receivedType;
        }
    }
}
