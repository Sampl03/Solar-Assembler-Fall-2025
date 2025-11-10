using Solar.EntitySystem;
using Solar.EntitySystem.Behavior;

namespace Solar.Asm.Engine.Model.Code
{
    public class Section : CodeEntity, IUniqueEntity
    {
        public IList<EntityHandle<Fragment>> Fragments { get; } = [];

        public Section() : base()
        { 
        }

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
