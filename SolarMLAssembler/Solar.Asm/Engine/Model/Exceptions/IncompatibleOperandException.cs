namespace Solar.Asm.Engine.Model.Exceptions
{
    public class IncompatibleOperandException : SmlaException
    {
        public Type? ExpectedType;
        public Type? ReceivedType;

        public IncompatibleOperandException()
        {
        }

        public IncompatibleOperandException(string message) : base(message)
        {
        }

        public IncompatibleOperandException(string message, Exception inner) : base(message, inner)
        {
        }

        public IncompatibleOperandException(string message, Type expectedType, Type receivedType) : this(message)
        {
            ExpectedType = expectedType;
            ReceivedType = receivedType;
        }

        public IncompatibleOperandException(string message, Type expectedType, Type receivedType, Exception inner) : this(message, inner)
        {
            ExpectedType = expectedType;
            ReceivedType = receivedType;
        }
    }
}
