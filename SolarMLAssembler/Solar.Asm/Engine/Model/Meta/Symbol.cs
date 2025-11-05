using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Meta
{
    public abstract class Symbol : ModelEntity
    {
        protected Symbol(EntityManager owningTable) : base(owningTable)
        {
        }
    }
}
