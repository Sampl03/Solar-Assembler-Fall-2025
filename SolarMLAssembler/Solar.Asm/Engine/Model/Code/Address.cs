using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Code
{
    public abstract class Address : CodeEntity
    {
        protected Address(EntityManager owningTable) : base(owningTable)
        {
        }
    }
}
