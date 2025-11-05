namespace Solar.EntitySystem.Behavior
{
    /// <summary>
    /// Non-generic superclass of IUniqueEntity<br/>
    /// 
    /// </summary>
    public interface IUniqueEntityBase : IMergeableBase
    {

    }

    public interface IUniqueEntity<T> : IUniqueEntityBase, IMergeable<T> where T : ModelEntity
    {

    }
}
