using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Program
{
    public abstract class Expression : ProgramEntity
    {
        protected Expression(EntityManager owningTable) : base(owningTable)
        {
        }
    }
}
