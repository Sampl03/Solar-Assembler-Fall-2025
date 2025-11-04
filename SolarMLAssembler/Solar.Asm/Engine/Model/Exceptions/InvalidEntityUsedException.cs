namespace Solar.Asm.Engine.Model.Exceptions
{
    public class InvalidEntityUsedException : SmlaException
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
