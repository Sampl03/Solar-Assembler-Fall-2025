using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model
{
    /// <summary>
    /// Represents a class that has namespace-qualified name entities of type <typeparamref name="T"/>
    /// </summary>
    /// <remarks>
    /// <see cref="NamespaceLookupService"/> can be used to implement scoped search on implementors of this interface
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public interface INamespaceSearchable<T> where T : ModelEntity
    {
        /// <param name="name"></param>
        /// <returns>
        /// The unique entity with this exact qualified name, or null if there isn't one.<br/>
        /// </returns>
        public T? GetUnique(QualifiedName name);
    }
}
