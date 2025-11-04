namespace Solar.Asm.Engine.Model.Exceptions
{
    public class IrreplaceableEntityException : SmlaException
    {
        public IrreplaceableEntityException() { }

        public IrreplaceableEntityException(string message) : base(message) { }

        public IrreplaceableEntityException(string? message, Exception inner) : base(message, inner) { }
    }
}
