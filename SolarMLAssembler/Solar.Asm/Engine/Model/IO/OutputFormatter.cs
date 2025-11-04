using Solar.Asm.Engine.Model.Entity;

namespace Solar.Asm.Engine.Model.IO
{
    public abstract class OutputFormatter : ModelEntity
    {
        protected OutputFormatter(EntityManager owningTable) : base(owningTable)
        {
        }
    }
}
