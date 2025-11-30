namespace Solar.Asm.Engine.Model.Exceptions
{
    public class SmlaIncompatibleArchitectureException : SmlaException
    {
        public SmlaIncompatibleArchitectureException()
        {
        }

        public SmlaIncompatibleArchitectureException(string message) : base(message)
        {
        }

        public SmlaIncompatibleArchitectureException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
