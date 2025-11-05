using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Program
{
    public abstract class ProgramEntity : ModelEntity
    {
        protected ProgramEntity(EntityManager owningTable) : base(owningTable) { }
    }
}
