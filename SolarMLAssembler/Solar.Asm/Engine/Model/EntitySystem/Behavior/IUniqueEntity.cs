namespace Solar.EntitySystem.Behavior
{
    /// <summary>
    /// Represents an entity that should be unique within its manager.<br/>
    /// The manager will automatically merge equivalent entities together. They must be of the same type
    /// </summary>
    /// <remarks>
    /// Merge behaviour defaults to doing nothing, although this be customized.
    /// </remarks>
    public interface IUniqueEntity : IMergeable
    {
        bool IMergeable.CanMerge(IMergeable other) => true;
        void IMergeable.Merge(IMergeable other) { }
        int EntityHash();
        bool EntityEquivalent(IUniqueEntity other);
    }
}
