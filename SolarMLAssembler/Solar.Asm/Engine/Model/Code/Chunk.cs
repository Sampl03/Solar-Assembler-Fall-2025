using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Code
{
    public abstract class Chunk : CodeEntity
    {
        protected Chunk(EntityManager owningTable) : base(owningTable) { }
    }
}
