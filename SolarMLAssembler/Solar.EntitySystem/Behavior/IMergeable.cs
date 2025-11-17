using Solar.EntitySystem.Exceptions;

namespace Solar.EntitySystem.Behavior
{
    /// <summary>
    /// Represents a class whose instances can be merged.
    /// </summary>
    /// <remarks>
    /// The implementor need not be an Entity.
    /// </remarks>
    public interface IMergeable
    {
        /// <summary>
        /// Indicates whether this mergeable can merge into another.
        /// </summary>
        /// <param name="destination">The destination mergeable to merge into</param>
        /// <returns>
        /// <see langword="true"/> if the two mergeables can merge<br/>
        /// <see langword="false"/> if not
        /// </returns>
        bool CanMergeInto(IMergeable destination);

        /// <summary>
        /// Merge this mergeable into <paramref name="destination"/>
        /// </summary>
        /// <remarks>
        /// Should throw <see cref="CannotMergeException"/> if the two entities cannot be merged.
        /// </remarks>
        /// <param name="destination">The destination mergeable to merge into</param>
        /// <exception cref="CannotMergeException"></exception>
        void MergeInto(IMergeable destination);
    }
}
