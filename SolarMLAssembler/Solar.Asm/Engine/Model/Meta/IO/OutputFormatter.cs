using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Meta.IO
{
    public abstract class OutputFormatter : ModelEntity
    {
        protected OutputFormatter(EntityManager owningTable) : base(owningTable)
        {
        }
    }
}
