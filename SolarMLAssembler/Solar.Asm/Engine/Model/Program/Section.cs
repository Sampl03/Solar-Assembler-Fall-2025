using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Program
{
    public abstract class Section : ProgramEntity
    {
        protected Section(EntityManager owningTable) : base(owningTable)
        {
        }
    }
}
