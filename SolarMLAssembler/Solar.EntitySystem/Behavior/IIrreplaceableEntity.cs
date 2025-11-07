namespace Solar.EntitySystem.Behavior
{
    /// <summary>
    /// An entity of this type cannot be replaced in the data model
    /// </summary>
    /// <remarks>
    /// If an entity implementing this interface also implements IUniqueEntity, equivalent instances will be merged
    /// </remarks>
    public interface IIrreplaceableEntity { }
}
