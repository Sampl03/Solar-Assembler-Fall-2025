using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Code
{
    public abstract class Chunk : CodeEntity
    {
        // Fragment and PreviousChunk should only be set by the containing Fragment.
        internal EntityHandle<Fragment>? _fragment;
        internal EntityHandle<Chunk>? _prevChunk;
        public Fragment? Fragment => _fragment?.Ref;
        public Chunk? PreviousChunk => _prevChunk?.Ref;

        protected Chunk() : base() { }

        public abstract override IReadOnlyList<byte> EmitBytes();

        public abstract override BinaryPatch[] EmitPatches();

        public sealed override ulong CalculateMemCellOffset()
        {
            GuardValidity();

            if (PreviousChunk is null)
                return 0;

            return PreviousChunk!.CalculateMemCellOffset() + PreviousChunk!.CalculateMemSize();
        }

        public sealed override ulong CalculateMemCellVirtualAddress()
        {
            return Fragment!.CalculateMemCellVirtualAddress() + CalculateMemCellOffset();
        }

        protected override void OnInvalidated()
        {
            _fragment?.Dispose();
            _fragment = null;

            _prevChunk?.Dispose();
            _prevChunk = null;
        }
    }
}
