using Solar.Asm.Engine.Model.Entity;

namespace Solar.Asm.Engine.Model
{
    public abstract class Chunk : ProgramEntity
    {
        protected Chunk(EntityManager owningTable) : base(owningTable) { }
    }
}
