using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Code
{
    /// <summary>
    /// Represents any entity that can emit data as bytes.
    /// </summary>
    public abstract class CodeEntity : ModelEntity
    {
        /// <summary>
        /// Indicates whether this entity requires recalculation as of the latest operation,<br/>
        /// such as when an expression's value may still vary (e.g. symbols)
        /// </summary>
        public abstract bool NeedsRecalculation();

        /// <summary>
        /// Forces the entity to recalculate instead of using cached representation on any further calls
        /// </summary>
        public abstract void RequireRecalculation();

        /// <returns>
        /// The byte representation of the entity as a byte list
        /// </returns>
        public abstract IReadOnlyList<byte> EmitBytes();

        /// <returns>
        /// The number of emitted memory cells.<br/>
        /// Note that this value is not necessarily in bytes, but in memory cells defined by the architecture
        /// </returns>
        /// <remarks>=
        /// By default, this is the size of whatever <see cref="EmitBytes"/> returns,
        ///  divided by the number of bytes in a memory cell for the specified architecture
        /// </remarks>
        public virtual ulong CalculateMemSize() => (ulong)(
            EmitBytes().LongCount() /
            ((Program)OwningTable!.Context).ArchSpecs.MemoryCellSizeInBytes
        );

        /// <returns>
        /// The offset of this entity from its parent's address in memory cells (if applicable)<br/>
        /// Note that this value is not necessarily in bytes, but in memory cells defined by the architecture
        /// </returns>
        public abstract ulong CalculateMemCellOffset();
    }
}
