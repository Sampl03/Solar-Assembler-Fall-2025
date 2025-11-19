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

        /// <returns>The number of emitted bytes. By default, this is the size of whatever <see cref="EmitBytes"/> returns</returns>
        public virtual long CalculateByteSize() => EmitBytes().LongCount();

        /// <returns>The offset of this entity from its parent's address in bytes (if applicable)</returns>
        public abstract long CalculateByteOffset();
    }
}
