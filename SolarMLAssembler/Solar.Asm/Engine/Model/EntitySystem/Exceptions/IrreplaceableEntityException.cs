namespace Solar.EntitySystem.Exceptions
{
    public class IrreplaceableEntityException : EntitySystemException
    {
        public IrreplaceableEntityException() { }

        public IrreplaceableEntityException(string message) : base(message) { }

        public IrreplaceableEntityException(string? message, Exception inner) : base(message, inner) { }
    }
}
