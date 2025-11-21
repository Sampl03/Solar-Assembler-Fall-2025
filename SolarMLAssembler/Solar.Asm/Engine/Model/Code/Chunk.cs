using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Code
{
    public abstract class Chunk : CodeEntity
    {
        // Fragment and PreviousChunk should only be set by the containing Fragment.
        public EntityHandle<Fragment>? Fragment { get; internal set; }
        public EntityHandle<Chunk>? PreviousChunk { get; internal set; }

        protected Chunk() : base() { }

        /// <returns>The offset in bytes of this chunk from the start of its containing fragment</returns>
        public override ulong CalculateMemCellOffset()
        {
            GuardValidity();

            if (PreviousChunk is null)
                return 0;

            return PreviousChunk.Ref!.CalculateMemCellOffset() + PreviousChunk.Ref!.CalculateMemSize();
        }
    }
}
