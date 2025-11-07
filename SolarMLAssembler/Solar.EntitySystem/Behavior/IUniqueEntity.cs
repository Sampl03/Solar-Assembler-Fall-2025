using Solar.EntitySystem.Exceptions;

namespace Solar.EntitySystem.Behavior
{
    /// <summary>
    /// Represents an entity that should be unique within its manager<br/>
    /// <br/>
    /// Merging will only apply when two managers merge and they contain <see cref="IUniqueEntity"/> entities that are equivalent to each other<br/>
    /// The default behaviour is such cases is to do nothing, although it can be customized.<br/>
    /// </summary>
    /// <remarks>
    /// <b>NOTE:</b><br/>
    /// Such entities should not be directly constructed and initialised.<br/>
    /// Instead, create an uninitialised template entity and pass it to the <see cref="UniqueEntity.Make{T}(T)"/> factory function.
    /// </remarks>
    public interface IUniqueEntity : IMergeable
    {
        /// <param name="other">The other entity</param>
        /// <returns>
        /// (Default behaviour)<br/>
        /// <see langword="true"/> if this entity and <paramref name="other"/> are of the same run-time type<br/>
        /// <see langword="false"/> otherwise
        /// </returns>
        bool IMergeable.CanMerge(IMergeable other)
        {
            if (GetType() != other.GetType())
                return false;

            return EntityEquivalent((ModelEntity)other);
        }

        /// <summary>
        /// Merge this entity with another entity. Used in <see cref="EntityManager"/> during manager merges.<br/>
        /// </summary>
        /// <remarks>
        /// Throws <see cref="CannotMergeException"/> if the two entities are not of the same type and equivalent
        /// </remarks>
        /// <param name="other"></param>
        /// <exception cref="CannotMergeException"></exception>
        void IMergeable.Merge(IMergeable other)
        {
            if (!CanMerge(other))
                throw new CannotMergeException($"Cannot merge type '{GetType().FullName}' with type '{other.GetType().FullName}'");
        }

        /// <summary>
        /// Return a quick hashcode to narrow down the number of entities to verify.
        /// Entities that are equivalent should have the same hash value.
        /// </summary>
        /// <remarks>
        /// See also:<br/>
        /// - <seealso cref="EntityEquivalent(ModelEntity)"/>
        /// </remarks>
        /// <returns>
        /// A hash value for this entity that should be equal for equivalent entities.<br/>
        /// Note that two non-equivalent entities may not be equivalent, so an equivalence function
        ///  should be used as a final check
        /// </returns>
        int EntityHash();

        /// <summary>
        /// Determine whether two entities are actually equivalent
        /// </summary>
        /// <remarks>
        /// It is the reponsibility of the implementor to check <paramref name="other"/>'s type
        /// </remarks>
        /// <param name="other">The other entity</param>
        /// <returns>
        /// Whether or not the entities are equivalent
        /// </returns>
        bool EntityEquivalent(ModelEntity other);
    }

    /// <summary>
    /// Helper class of the IUniqueEntity interface
    /// </summary>
    public static class UniqueEntity
    {
        /// <summary>
        /// Factory method for unique entities.<br/>
        /// This method will return a valid entity which is equivalent to <paramref name="template"/>, while insuring uniqueness.
        /// </summary>
        /// <remarks>
        /// Throws <see cref="EntityTemplateInitialisedException"/> if <paramref name="template"/> is not is the <see cref="EntityState.Uninitialised"/> state.
        /// </remarks>
        /// <typeparam name="TUnique"></typeparam>
        /// <param name="template">The unitialised template instance to search for</param>
        /// <returns>
        /// A valid entity, whether it be reused or a new one
        /// </returns>
        public static TUnique Make<TUnique>(TUnique template) where TUnique : ModelEntity, IUniqueEntity
        {
            // Insure uninitialised state
            if (template.State != EntityState.Uninitialised)
                throw new EntityTemplateInitialisedException("Cannot used a non-Unitialised entity as a template for the Make function.");

            // Cache the hash for the template
            var templateHash = template.EntityHash();

            // Search for equivalent entities
            var otherEntity = template
                .OwningTable
                .SearchEntities<TUnique>(e => e.EntityHash() == templateHash)
                .Where(template.EntityEquivalent)
                .ToArray();

            // There should be 0 or 1 because of EntityManager's registration rules. We do not check this

            // If there is already an entity, we return it.
            if (otherEntity.Count() != 0)
                return otherEntity[0];

            // Otherwise initialise the template and return it
            template.Initialise();
            return template;
            
        }
    }
}
