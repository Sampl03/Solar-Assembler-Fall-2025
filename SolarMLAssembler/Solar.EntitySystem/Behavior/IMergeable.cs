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
        /// Indicates whether this mergeable can merge with another.
        /// </summary>
        /// <param name="other">The other mergeable</param>
        /// <returns>
        /// <see langword="true"/> if the two mergeables can merge<br/>
        /// <see langword="false"/> if not
        /// </returns>
        bool CanMerge(IMergeable other);

        /// <summary>
        /// Merge two mergeables.<br/>
        /// This method should call <see cref="WasMerged()"/> on <paramref name="other"/> if it has additional state to cleanup.
        /// </summary>
        /// <remarks>
        /// Should throw <see cref="CannotMergeException"/> if the two entities cannot be merged.
        /// </remarks>
        /// <param name="other">The other mergeable</param>
        /// <exception cref="CannotMergeException"></exception>
        void Merge(IMergeable other);

        /// <summary>
        /// Tells a mergeable that it has been merged and needs to clean up.<br/>
        /// Should be called as necessary in <see cref="Merge(IMergeable)"/>.
        /// </summary>
        void WasMerged() { }
    }
}
