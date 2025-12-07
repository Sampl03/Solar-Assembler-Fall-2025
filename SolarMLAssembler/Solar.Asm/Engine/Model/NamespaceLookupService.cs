using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model
{
    /// <summary>
    /// Provides a lookup table engine for classes implemented <see cref="INamespaceSearchable{T}"/>
    /// </summary>
    public static class NamespaceLookupService
    {
        /// <summary>
        /// Tries to retrieve a matching entity, climbing up the namespace as needed.
        /// </summary>
        /// <remarks>
        /// Note that <paramref name="nameToSearch"/> will stay constant,
        /// such that searching (A::B::C, D::E) will attempt resolution in the following order:<br/>
        /// <list type="number">
        /// <item>A::B::C::D::E</item>
        /// <item>A::B::D::E</item>
        /// <item>A::D::E</item>
        /// <item>D::E</item>
        /// <item><see langword="null"/></item>
        /// </list>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="searchable"></param>
        /// <param name="currentNamespace"></param>
        /// <param name="nameToSearch"></param>
        /// <returns></returns>
        public static T? ResolveUnique<T>(INamespaceSearchable<T> searchable,
                                          QualifiedName currentNamespace,
                                          QualifiedName nameToSearch)
            where T : ModelEntity
        {
            T? result = null;
            while (currentNamespace.IsEmpty)
            {
                result = searchable.GetUnique(currentNamespace + nameToSearch);

                if (result != null)
                    break;

                currentNamespace = currentNamespace.Namespace;
            }

            return result;
        }
    }
}
