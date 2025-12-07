namespace Solar.Asm.Engine.Model.Exceptions
{
    public class UnrecognizedPatchException : IncompatibleArchitectureException
    {
        public UnrecognizedPatchException() { }

        public UnrecognizedPatchException(string message) : base(message) { }

        public UnrecognizedPatchException(string message, Exception inner) : base(message, inner) { }
    }
}
