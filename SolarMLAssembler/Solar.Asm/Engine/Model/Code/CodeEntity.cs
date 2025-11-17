using Solar.EntitySystem;
using System.Reflection.Metadata.Ecma335;

namespace Solar.Asm.Engine.Model.Code
{
    /// <summary>
    /// Represents any entity that can emit data as bytes.
    /// </summary>
    public abstract class CodeEntity : ModelEntity
    {
        /// <summary>
        /// Returns the byte representation of this entity's data.
        /// </summary>
        /// <remarks>
        /// The <paramref name="mustRecalculate"/> param indicates whether this entity requires recalculation,
        /// such as when a symbol couldn't be resolved or may be changed by a change in this instruction.
        /// </remarks>
        /// <param name="mustRecalculate">
        /// If <see langword="true"/>, indicates that this entity requires a recalculation
        /// such as if it refers to an uninitialised symbol or it changed size and refers to a symbol that may be affected.<br/>
        /// If the results don't require recalculation, this should be <see langword="false"/>
        /// </param>
        /// <returns>
        /// The byte representation of the entity as a byte enumerable
        /// </returns>
        public abstract IEnumerable<byte> EmitBytes(out bool mustRecalculate);

        /// <returns>The number of emitted bytes, if applicable</returns>
        public abstract long? CalculateByteSize();

        /// <returns>The virtual address of the start of the virtual bytes, if applicable</returns>
        public abstract long? CalculateAddress();
    }
}
