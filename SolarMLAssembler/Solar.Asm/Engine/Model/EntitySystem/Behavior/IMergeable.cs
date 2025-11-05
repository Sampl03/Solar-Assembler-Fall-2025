namespace Solar.EntitySystem.Behavior
{
    public interface IMergeableBase
    {
        bool CanMerge(IMergeableBase other);
        bool TryMerge(IMergeableBase other);
    }

    /// <summary>
    /// Represents a class whose instances can be merged.<br/>
    /// The implementor need not be an Entity.
    /// </summary>
    public interface IMergeable<T> : IMergeableBase where T : class
    {
        bool CanMerge(IMergeable<T> other);

        bool Merge(IMergeable<T> entity);
    }
}
