using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Code
{
    /// <summary>
    /// Represents any entity that can emit data as bytes.
    /// </summary>
    public abstract class CodeEntity : ModelEntity
    {
        /// <summary>
        /// The Program in which this code entity was declared
        /// </summary>
        public Program OwningProgram
        {
            get
            {
                GuardValidity();
                return (Program)OwningTable!.Context;
            }
        }

        /// <returns>
        /// The byte representation of the entity as a byte list
        /// </returns>
        public abstract IReadOnlyList<byte> EmitBytes();

        /// <returns>
        /// The binary patches emitted by this entity
        /// </returns>
        public abstract BinaryPatch[] EmitPatches();

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
        /// The offset of this entity from its parent's address in memory cells<br/>
        /// Note that this value is not necessarily in bytes, but in memory cells defined by the architecture
        /// </returns>
        public abstract ulong CalculateMemCellOffset();

        /// <returns>
        /// The virtual address of this entity in memory cells<br/>
        /// Note that this value is not necessarily in bytes, but in memory cells defined by the architecture
        /// </returns>
        public abstract ulong CalculateMemCellVirtualAddress();
    }
}
