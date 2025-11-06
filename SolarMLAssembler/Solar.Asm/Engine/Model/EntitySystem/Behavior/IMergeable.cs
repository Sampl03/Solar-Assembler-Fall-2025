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
        bool CanMerge(IMergeable other);
        void Merge(IMergeable other);
    }
}
