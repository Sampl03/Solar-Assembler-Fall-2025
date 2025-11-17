using Solar.EntitySystem;
using Solar.EntitySystem.Behavior;

namespace Solar.Asm.Engine.Model.Expressions
{
    public abstract class LiteralExpr<TReturn>
        : Expression<TReturn>, IIrreplaceableEntity, IUniqueEntity
        where TReturn : LiteralExpr<TReturn>
    {
        public virtual bool EntityEquivalent(ModelEntity other)
        {
            throw new NotImplementedException();
        }

        public virtual int EntityHash()
        {
            throw new NotImplementedException();
        }
    }
}
