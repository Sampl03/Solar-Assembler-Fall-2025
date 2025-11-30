namespace Solar.Asm.Engine.Model.Exceptions
{
    public class SmlaInvalidPatchException : SmlaIncompatibleArchitectureException
    {
        public SmlaInvalidPatchException() { }

        public SmlaInvalidPatchException(string message) : base(message) { }

        public SmlaInvalidPatchException(string message, Exception inner) : base(message, inner) { }
    }
}
