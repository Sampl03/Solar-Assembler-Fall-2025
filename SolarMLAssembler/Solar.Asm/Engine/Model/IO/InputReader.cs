using Solar.Asm.Engine.Model.Entity;

namespace Solar.Asm.Engine.Model.IO
{
    public abstract class InputReader : ModelEntity
    {
        protected InputReader(EntityManager owningTable) : base(owningTable)
        {
        }
    }
}
