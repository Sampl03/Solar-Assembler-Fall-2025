using Solar.EntitySystem;
using Solar.EntitySystem.Behavior;

namespace Solar.Asm.Engine.Model.Code
{
    public class Section(string name, SectionFlags flags) : CodeEntity(), IUniqueEntity
    {
        public IList<EntityHandle<Fragment>> Fragments { get; } = [];

        public string Name { get; init; } = name;

        public SectionFlags Flags { get; init; } = flags;

        public virtual bool CanMergeInto(IMergeable destination)
        {
            if (destination is not Section)
                return false;

            var destSection = (Section)destination;

            if (Name != destSection.Name)
                return false;

            return Flags.CanMergeInto(destSection.Flags);
        }

        public virtual void MergeInto(IMergeable destination)
        {
            throw new NotImplementedException();
        }

        public virtual bool EntityEquivalent(ModelEntity other)
        {
            if (other is not Section)
                return false;

            var otherSection = (Section)other;

            return Flags.CanMergeInto(otherSection.Flags);
        }

        public virtual int EntityHash()
        {
            return Name.GetHashCode();
        }

        public override IEnumerable<byte> EmitBytes(out bool mustRecalculate)
        {
            throw new NotImplementedException();
        }

        public override long? CalculateByteSize()
        {
            throw new NotImplementedException();
        }

        public override long? CalculateAddress()
        {
            throw new NotImplementedException();
        }
    }
}
