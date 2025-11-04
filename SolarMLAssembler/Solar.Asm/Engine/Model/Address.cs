using Solar.Asm.Engine.Model.Entity;

namespace Solar.Asm.Engine.Model
{
    public abstract class Address : ProgramEntity
    {
        protected Address(EntityManager owningTable) : base(owningTable)
        {
        }
    }
}
