namespace Solar.Asm.Engine.Model.Exceptions
{
    public class IncompatibleArchitectureException : SmlaException
    {
        public IncompatibleArchitectureException()
        {
        }

        public IncompatibleArchitectureException(string message) : base(message)
        {
        }

        public IncompatibleArchitectureException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
