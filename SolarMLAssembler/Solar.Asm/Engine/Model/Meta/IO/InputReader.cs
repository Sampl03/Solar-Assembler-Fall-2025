using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Meta.IO
{
    public abstract class InputReader : ModelEntity
    {
        protected InputReader(EntityManager owningTable) : base(owningTable)
        {
        }
    }
}
