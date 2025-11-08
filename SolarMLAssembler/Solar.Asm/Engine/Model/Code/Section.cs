using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Code
{
    public abstract class Section : CodeEntity
    {
        protected Section(EntityManager owningTable) : base(owningTable)
        {
        }
    }
}
