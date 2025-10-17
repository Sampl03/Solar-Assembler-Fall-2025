namespace Solar.Asm.Engine.Model.Entity
{
    // Represents an entity in the model which can be accessed via an opaque handle
    public abstract class ModelEntity
    {
        public uint ID { get; protected set; }
    }
}
