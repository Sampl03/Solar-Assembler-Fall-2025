namespace Solar.EntitySystem.Exceptions
{
    public class EntityTemplateInitialisedException : EntitySystemException
    {
        public EntityTemplateInitialisedException()
        {
        }

        public EntityTemplateInitialisedException(string message) : base(message)
        {
        }

        public EntityTemplateInitialisedException(string? message, Exception inner) : base(message, inner)
        {
        }
    }
}
