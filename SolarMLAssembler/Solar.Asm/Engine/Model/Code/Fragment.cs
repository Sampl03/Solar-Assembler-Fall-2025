using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Code
{
    public class Fragment : CodeEntity
    {
        public IList<EntityHandle<Chunk>> Chunks { get; } = [];

        public EntityHandle<Section>? Section { get; protected set; }
        public EntityHandle<Fragment>? PreviousFragment { get; protected set; }

        public Fragment() : base()
        {
        }

        protected override bool OnValidityGuard()
        {
            return Section is not null;
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
