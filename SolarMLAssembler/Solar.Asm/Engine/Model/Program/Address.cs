using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Program
{
    public abstract class Address : ProgramEntity
    {
        protected Address(EntityManager owningTable) : base(owningTable)
        {
        }
    }
}
