using Solar.Asm.Engine.Model.Entity;

namespace Solar.Asm.Engine.Model
{
    public abstract class Expression : ProgramEntity
    {
        protected Expression(EntityManager owningTable) : base(owningTable)
        {
        }
    }
}
