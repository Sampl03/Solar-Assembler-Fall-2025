using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Program
{
    public abstract class Chunk : ProgramEntity
    {
        protected Chunk(EntityManager owningTable) : base(owningTable) { }
    }
}
