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

        /// <returns>The number of emitted bytes, if applicable</returns>
        public abstract long? CalculateByteSize();

        /// <returns>The virtual address of the start of the virtual bytes, if applicable</returns>
        public abstract long? CalculateAddress();
    }
}
