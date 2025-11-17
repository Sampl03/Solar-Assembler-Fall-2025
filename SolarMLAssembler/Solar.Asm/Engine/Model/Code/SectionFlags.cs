using Solar.EntitySystem.Behavior;
using Solar.EntitySystem.Exceptions;

namespace Solar.Asm.Engine.Model.Code
{
    public record SectionFlags : IMergeable
    {
        /// <summary>
        /// The alignment for this section, as an exponent of a power of 2.<br/>
        /// <br/>
        /// - 0 = 2^0 = 1<br/>
        /// - 1 = 2^1 = 2<br/>
        /// - 2 = 2^2 = 4<br/>
        /// - ...
        /// </summary>
        public byte   Alignment     { get; init; } = 0;

        /// <summary>
        /// The byte value pattern to use during padding.<br/>
        /// The pattern is repeated until the space is filled<br/>
        /// <br/>
        /// - [1, 2, 3] = 0x01 0x02 0x03 0x1 0x2 ...
        /// </summary>
        public byte[] PadValue      { get; init; } = [0x00];

        /// <summary>
        /// Whether or not this section can be merged with other sections of the same type and flags
        /// </summary>
        public bool   IsMergeable   { get; init; } = false;

        /// <summary>
        /// Indicates that this section contains data
        /// </summary>
        public bool   IsData        { get; init; } = false;

        /// <summary>
        /// Indicates that this section is uninitialised
        /// </summary>
        public bool   IsUnitialised { get; init; } = false;

        /// <summary>
        /// Indicates that this section contains executable code/data
        /// </summary>
        public bool   IsExecutable  { get; init; } = false;

        /// <summary>
        /// Indicates that this section can be written to
        /// </summary>
        public bool   IsWriteable   { get; init; } = false;

        public virtual bool CanMerge(IMergeable other)
        {
            if (!IsMergeable)
                return false;

            return Equals(other);
        }

        public virtual void Merge(IMergeable other)
        {
            if (!CanMerge(other))
                throw new CannotMergeException("Could not merge SectionFlags instance with other mergeable", this, other);

            // Equality, we don't need to change anything
            return;
        }
    }
}
