using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Expressions
{
    public abstract class Expression<TReturn>(EntityManager owningTable)
        : ModelEntity(owningTable)
        where TReturn : ModelEntity
    { }
}
