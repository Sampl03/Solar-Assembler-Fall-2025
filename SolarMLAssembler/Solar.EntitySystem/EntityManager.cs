using Solar.EntitySystem.Behavior;
using Solar.EntitySystem.Exceptions;
using Solar.EntitySystem.Utils;

namespace Solar.EntitySystem
{
    /// <summary>
    /// A manager for entities.
    /// 
    /// Tracks entities and provides functionality for querying and obtaining handles to them.
    /// </summary>
    public sealed class EntityManager : IMergeable
    {
        /// <summary>The list of entities in this manager</summary>
        private readonly List<ModelEntity> _entities = [];
        /// <summary>A map of entities to their assigned handles</summary>
        private readonly Dictionary<ModelEntity, List<WeakReference<EntityHandleBase>>> _entityHandles = [];

        /// <summary>The most generic type of referent this manager can store</summary>
        public readonly Type MaximalEntityType;

        /// <summary>
        /// Creates a new <see cref="EntityManager"/> to store entities and their handles
        /// </summary>
        /// <remarks>
        /// Throws <see cref="ArgumentException"/> if <paramref name="maximalEntityType"/> is not a type assignable to <see cref="ModelEntity"/>."/>
        /// </remarks>
        /// <param name="maximalEntityType">The most generic referent type </param>
        /// <exception cref="ArgumentException"></exception>
        public EntityManager(Type maximalEntityType)
        {
            if (!typeof(ModelEntity).IsAssignableFrom(maximalEntityType))
                throw new ArgumentException("maximalEntityType must be a type derived from ModelEntity", nameof(maximalEntityType));

            MaximalEntityType = maximalEntityType;
        }

        /// <summary>Returns the number of </summary>
        /// <returns></returns>
        public int GetEntityCount() => _entities.Where(entity => entity.IsValid).Count();

        /// <summary>
        /// Searches for valid entities of a specific type, with filtering support
        /// </summary>
        /// <typeparam name="TEntity">The specific type</typeparam>
        /// <param name="predicate">An optional search filter function to apply</param>
        /// <returns>
        /// An enumerable of the matching entities
        /// </returns>
        public IEnumerable<TEntity> SearchEntities<TEntity>(Func<TEntity, bool>? predicate = null) where TEntity : ModelEntity
        {
            return _entities
                .OfType<TEntity>()
                .Where(entity => entity.IsValid)
                .Where(predicate ?? (e => true));
        }

        /// <summary>
        /// Searches for valid entities, with filtering support
        /// </summary>
        /// <param name="predicate">An optional search filter function to apply</param>
        /// <returns>
        /// An enumerable of the matching entities
        /// </returns>
        public IEnumerable<ModelEntity> SearchEntities(Func<ModelEntity, bool>? predicate = null)
        {
            return _entities
                .Where(entity => entity.IsValid)
                .Where(predicate ?? (e => true));
        }

        /// <summary>
        /// Find the unique entity equivalent to <paramref name="template"/> in this entity manager.<br/>
        /// This may return the template itself if it has been initialised within the manager
        /// </summary>
        /// <remarks>
        /// Throws <see cref="UniquenessConstraintFailedException"/> if the entity is IUniqueEntity and another duplicate already exists (incorrect initialisation)
        /// </remarks>
        /// <param name="template">The template instance to use for equivalence checking</param>
        /// <returns>
        /// The sole equivalent entity, or <see langword="null"/> if there is none
        /// </returns>
        /// <exception cref="UniquenessConstraintFailedException"></exception>
        public IUniqueEntity? FindEquivalentEntity(IUniqueEntity template)
        {
            Type entityType = template.GetType();
            int entityHash = template.EntityHash();

            var otherEntities = _entities
                .Where(e => e is IUniqueEntity)
                .Where( // Find all other entities of the same type
                    e => entityType.Equals(e.GetType()))
                .Where( // Which have an equivalent hash
                    e => ((IUniqueEntity)e).EntityHash() == entityHash)
                .Where( // Which are equivalent
                    e => ((IUniqueEntity)e).EntityEquivalent((ModelEntity)template))
                .Cast<IUniqueEntity>();

            if (otherEntities.Count() > 0)
                throw new UniquenessConstraintFailedException("There cannot be more than one equivalent instances of the same IUniqueEntity.");

            return otherEntities.FirstOrDefault();
        }

        /// <param name="other">The other manager to merge with</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="other"/> is a compatible <see cref="EntityManager"/>. They must have the same maximal type and not be the same manager<br/>
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool CanMerge(IMergeable other)
        {
            if (other is not EntityManager)
                return false;

            if (MaximalEntityType != ((EntityManager)other).MaximalEntityType)
                return false;

            return !ReferenceEquals(this, other);
        }

        /// <summary>
        /// Transfers all the objects of the <paramref name="other"/> manager to this manager, provided that they are compatible.<br/>
        /// Unique entities with equivalent instances in both managers will be merged together
        /// </summary>
        /// <remarks>
        /// Throws <see cref="CannotMergeException"/> if <paramref name="other"/> is not a compatible manager<br/>
        /// See <seealso cref="CanMerge(IMergeable)"/>
        /// </remarks>
        /// <param name="other"></param>
        /// <exception cref="CannotMergeException"></exception>
        public void Merge(IMergeable other)
        {
            if (!CanMerge(other))
                throw new CannotMergeException("EntityManager instance could not merge with another instance", this, other);

            var otherManager = (EntityManager)other;

            // Iterate over the entities of the other manager, transferring their handles
            CleanupAllHandles();
            otherManager.CleanupAllHandles();

            for (int i = otherManager._entities.Count - 1; i >= 0; i--)
            {
                ModelEntity entityToTransfer = otherManager._entities[i];
                var entityHandlesToTransfer = otherManager._entityHandles[entityToTransfer];

                // For unique entities, verify if there's another equivalent, and merge as needed
                if (entityToTransfer is IUniqueEntity)
                {
                    // Find the equivalent entity if it exists
                    var existingEntity = FindEquivalentEntity((IUniqueEntity)entityToTransfer);

                    // Merge into it if it exists
                    if (existingEntity is not null)
                    {
                        existingEntity.Merge((IMergeable)entityToTransfer);

                        // Transfer handles
                        _entityHandles[(ModelEntity)existingEntity].AddRange(entityHandlesToTransfer);
                        foreach (var handle in entityHandlesToTransfer.GetLiveTargets())
                            handle.ReplaceReferent((ModelEntity)existingEntity);
                        entityHandlesToTransfer.Clear();

                        // Remove from the old manager
                        otherManager._entities.RemoveAt(i);
                        otherManager._entityHandles.Remove(entityToTransfer);

                        continue; // We don't need to move the old entity in this case
                    }
                }

                // Move the entity to the current manager
                _entities.Add(entityToTransfer);
                _entityHandles[entityToTransfer] = [.. entityHandlesToTransfer];

                // Remove from the old manager
                otherManager._entities.RemoveAt(i);
                otherManager._entityHandles.Remove(entityToTransfer);

            }
        }

        /// <summary>Returns the number of valid handles</summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal int GetHandleCount(ModelEntity entity)
        {
            if (!_entityHandles.ContainsKey(entity))
                return -1;

            CleanupHandles(entity);
            return _entityHandles[entity].Count;
        }

        /// <summary>Cleanup an entity's handle, removing references to disposed or invalidated handles</summary>
        /// <param name="entity">The entity to clean up the handles of</param>
        internal void CleanupHandles(ModelEntity entity)
        {
            if (!_entityHandles.ContainsKey(entity))
                return;

            // Remove invalid or disposed handles
            for (int i = _entityHandles[entity].Count - 1; i >= 0; i--)
                if (!_entityHandles[entity][i].TryGetTarget(out EntityHandleBase? handle) || !handle.IsValid)
                    _entityHandles[entity].RemoveAt(i);
        }

        /// <summary>Cleanup all entities' handles, removing references to disposed or invalidated handles</summary>
        /// <remarks>This may be an expensive operation. Use sparingly.</remarks>
        internal void CleanupAllHandles()
        {
            foreach (var entity in _entityHandles.Keys)
                CleanupHandles(entity);
        }

        /// <summary>
        /// Internal function to register an referent with this manager.
        /// </summary>
        /// <remarks>
        /// Should only be called by the referent itself.<br/>
        /// <br/>
        /// Throws <see cref="IncompatibleEntityException"/> if the referent is not compatible with this manager's maximal type.<br/>
        /// Throws <see cref="UniquenessConstraintFailedException"/> if the entity is IUniqueEntity and another duplicate already exists (incorrect initialisation)
        /// </remarks>
        /// <param name="entity">The referent to add</param>
        /// <exception cref="IncompatibleEntityException"></exception>
        /// <exception cref="UniquenessConstraintFailedException"></exception>
        /// <returns><see langword="true"/> if the referent was added, <see langword="false"/> if it was already present</returns>
        internal bool RegisterEntity(ModelEntity entity)
        {
            if (!MaximalEntityType.IsInstanceOfType(entity))
                throw new IncompatibleEntityException(
                    $"Entity of type '{entity.GetType().FullName}' is not assignable to EntityManager's maximal entity type '{MaximalEntityType.FullName}'",
                    MaximalEntityType, entity.GetType()
                );

            // Return false if the referent is already stored
            if (_entityHandles.ContainsKey(entity))
                return false;

            // If the entity is IUniqueEntity, check for duplicates
            if (entity is IUniqueEntity)
            {
                // This function throws if a duplicate exists
                FindEquivalentEntity((IUniqueEntity)entity);
            }

            // Add the referent and create empty handle list
            _entities.Add(entity);
            _entityHandles[entity] = [];

            return true;
        }

        /// <summary>
        /// Internal function to unregister an entity from this manager.
        /// </summary>
        /// <remarks>
        /// Should only be called by the entity itself during invalidation.<br/>
        /// <br/>
        /// Throws <see cref="CannotRemoveException"/> if there are still open handles to the entity.
        /// </remarks>
        /// <param name="entity">The entity to remove</param>
        /// <returns>
        /// <see langword="true"/> if the entity was removed,
        /// <see langword="false"/> if the entity was not known to this manager
        /// </returns>
        /// <exception cref="CannotRemoveException"></exception>
        internal bool UnregisterEntity(ModelEntity entity)
        {
            // If we don't know this entity, return false early
            if (!_entityHandles.ContainsKey(entity))
                return false;

            // Check if there are remaining open handles in this entity
            CleanupHandles(entity); // Cleanup dead handles first
            if (_entityHandles[entity].Count > 0)
                throw new CannotRemoveException("Cannot invalidate referent because there are still open handles to it.");

            // We can now safely remove the entity
            _entityHandles.Remove(entity);
            _entities.Remove(entity);

            return true;
        }

        /// <summary>
        /// Internal function to learn the most generic entity type the specified entity can transfer its handles to.
        /// </summary>
        /// <remarks>
        /// This is equal to the most specified type of all the entity's handles
        /// </remarks>
        /// <param name="entity">The entity to query</param>
        /// <param name="maximalType">The most generic entity type the specified entity can transfer its handles to, or <see langword="null"/> if the manager did not manage the specified entity</param>
        /// <returns>
        /// <see langword="true"/> If the entity is managed by this manager<br/>
        /// <see langword="false"/> If the entity is not managed by this manager and its maximalType couldn't be found
        /// </returns>
        internal bool GetEntityMaximalReplacementType(ModelEntity entity, out Type? maximalType)
        {
            // If we don't know this entity, we can't check its handles
            if (!_entityHandles.ContainsKey(entity))
            {
                maximalType = null;
                return false;
            }

            // Cleanup dead references first then cache the references
            CleanupHandles(entity);
            var handleList = _entityHandles[entity];

            // Then, iterate over all handles
            maximalType = typeof(ModelEntity); // Must be at most a ModelEntity
            foreach (EntityHandleBase handle in handleList.GetLiveTargets())
            {
                // If we find a type that is more specified than the current maximal type, then it is our new maximal type
                if (maximalType.IsAssignableFrom(handle.EntityMaxType) && !ReferenceEquals(maximalType, handle.EntityMaxType))
                    maximalType = handle.EntityMaxType;
            }

            // Once we're done, we now have the most generic type compatible with all of the entity's handles
            return true;
        }

        internal bool CanReplaceEntityWith(ModelEntity oldEntity, ModelEntity newEntity)
        {
            // Both entities must be valid
            if (!(oldEntity.IsValid && newEntity.IsValid))
                return false;

            // Old entity must be in this manager
            if (oldEntity.OwningTable != this)
                return false;

            // Both entities must be in the same manager
            if (oldEntity.OwningTable != newEntity.OwningTable)
                return false;

            // Replaced entity must not be IIrreplaceableEntity
            if (oldEntity is IIrreplaceableEntity)
                return false;

            // Check should not be needed (oldEntity belongs this manager if we reach here, but failsafe)
            if (!GetEntityMaximalReplacementType(oldEntity, out Type? maximalType))
                return false;

            // All handles of the old entity must be reassignable to the new entity
            if (!maximalType!.IsInstanceOfType(newEntity))
                return false;

            return true;
        }

        /// <summary>
        /// Internal function to replace an entity by another compatible entity
        /// </summary>
        /// <remarks>
        /// Both entities must be managed by this manager<br/>
        /// <br/>
        /// Throws <see cref="IrreplaceableEntityException"/> if the entity being replaced is marked as irreplaceable<br/>
        /// Throws <see cref="IncompatibleEntityException"/> if the new entity is incompatible with at least one of the old entity's handles<br/>
        /// Throws <see cref="ManagerMismatchException"/> if the old and new entities are not of the same manager
        /// </remarks>
        /// <param name="oldEntity"></param>
        /// <param name="newEntity"></param>
        /// <returns>
        /// <see langword="true"/> if the entity was successfully replaced<br/>
        /// <see langword="false"/> if the entities were not managed by this manager
        /// </returns>
        /// <exception cref="IrreplaceableEntityException"></exception>
        /// <exception cref="IncompatibleEntityException"></exception>
        /// <exception cref="ManagerMismatchException"></exception>
        internal bool ReplaceEntity(ModelEntity oldEntity, ModelEntity newEntity)
        {
            // If one of the entities is invalid, throw an exception
            oldEntity.GuardValidity();
            newEntity.GuardValidity();

            // If the old and new entities are not both in this manager, throw an exception
            if (oldEntity.OwningTable != newEntity.OwningTable)
                throw new ManagerMismatchException("Tried to replace entity with an entity from a different manager.");

            // If old entity is non-repleaceable, throw an exceptino
            if (oldEntity is IIrreplaceableEntity)
                throw new IrreplaceableEntityException($"Entity of type '{oldEntity.GetType().FullName}' cannot be replaced.");

            // Find the most generic type compatible with all of oldEntity's handles
            //  Return value is false if oldEntity is not managed by this manager (and thus newEntity too)
            //  This will cleanup dead oldEntity handles
            bool isEntityManaged = GetEntityMaximalReplacementType(oldEntity, out Type? maximalCompatibleType);

            // If the function returned false, oldEntity is not in this manager and we cannot proceed
            if (!isEntityManaged || maximalCompatibleType is null) // Doing a null check silences the compiler, though isEntityManaged fulfills this purpose
                return false;

            // If newEntity is not compatible with the maximal compatible type, throw an exception
            if (!maximalCompatibleType.IsInstanceOfType(newEntity))
                throw new IncompatibleEntityException(
                    $"Entity of type '{newEntity.GetType().FullName}' is not assignable to EntityManager's maximal entity type '{maximalCompatibleType.FullName}'",
                    maximalCompatibleType, newEntity.GetType()
                );

            // Everything is compatible, transfer the handles
            CleanupHandles(newEntity); // cleanup newEntity handle too
            var oldHandles = _entityHandles[oldEntity]; // cache for faster lookup
            var newHandles = _entityHandles[newEntity];

            // There should be no dead handles left so we use GetLiveTargets() for conciseness
            foreach (EntityHandleBase oldHandle in oldHandles.GetLiveTargets())
                oldHandle.ReplaceReferent(newEntity);
            newHandles.AddRange(oldHandles);
            oldHandles.Clear();

            // Invalidate the old entity
            oldEntity.Invalidate();

            return true; // success!
        }

        /// <summary>
        /// Internal function to create a handle to a known entity, with a type at least as derived as <see cref="ModelEntity"/> and at most of the actual type of the referent
        /// </summary>
        /// <remarks>
        /// Should only be called by the <see cref="ModelEntity"/> extension <see cref="EntityExtensions.GetHandle{THandle}(THandle)"/><br/>
        /// </remarks>
        /// <typeparam name="THandle">The underlying type of the handle. Must be at least a <see cref="ModelEntity"/></typeparam>
        /// <param name="entity">The entity to generate a handle to</param>
        /// <param name="handleOut">The generated handle, if the entity was managed by this manager. Otherwise,<see langword="null"/></param>
        /// <returns>
        /// <see langword="true"/> if the handle was created successfully<br/>
        /// <see langword="false"/> if the manager does not manage this entity<br/>
        /// </returns>
        internal bool TryCreateHandle<THandle>(THandle entity, out EntityHandle<THandle>? handleOut) where THandle : ModelEntity
        {
            // If we don't know this entity, we cannot create a handle to it
            if (!_entityHandles.ContainsKey(entity))
            {
                handleOut = null;
                return false;
            }

            // We create the handle of the specified type and register it
            handleOut = new EntityHandle<THandle>(entity);
            _entityHandles[entity].Add(new WeakReference<EntityHandleBase>(handleOut));
            return true;
        }

        /// <summary>
        /// Internal function to remove an referent handle from the tracking list.
        /// </summary>
        /// <remarks>
        /// Should only be called by the valid handle itself during invalidation.<br/>
        /// <br/>
        /// Throws <see cref="InvalidStateException"/> if the handle was not known to this manager but referenced a managed entity.
        /// </remarks>
        /// <param name="handle"></param>
        /// <returns>
        /// <see langword="true"/> if the referent was removed,
        /// <see langword="false"/> if the referent was not known to this manager
        /// </returns>
        /// <exception cref="InvalidStateException"></exception>
        internal bool RemoveEntityHandle(EntityHandleBase handle)
        {
            ModelEntity? referent = handle.UntypedRef;

            // If the handle is invalid or its referent of an incompatible type, we cannot proceed
            if (referent == null || !MaximalEntityType.IsInstanceOfType(referent))
                return false;

            // If we don't know the referent this handle is associated with, return false early
            if (!_entityHandles.ContainsKey(referent))
                return false;

            // Cleanup dead handles first then cache for faster lookup
            CleanupHandles(referent);
            var handlesList = _entityHandles[referent];

            // Now we look for the handle in the list and remove it if found
            for (int i = handlesList.Count - 1; i >= 0; i--)
            {
                if (handlesList[i].TryGetTarget(out EntityHandleBase? referentHandle) && ReferenceEquals(handle, referentHandle))
                {
                    handlesList.RemoveAt(i);

                    // If we just removed the last handle, we can invalidate this entity
                    if (handlesList.Count == 0)
                        referent.Invalidate();

                    return true;
                }
            }                

            // If the handle was not found, then we have a free-floating handle not tracked by this manager
            // That should never be the case, so we throw an exception
            throw new InvalidStateException("Invalid state: the provided handle referenced an entity but was not tracked by the entity's manager. Aborting.");
        }
    }

    /// <summary>
    /// Provides extension methods for entities to interact with the EntityManager
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Create a handle of type <typeparamref name="THandle"/> to this entity
        /// </summary>
        /// <remarks>
        /// <typeparamref name="THandle"/> must be a superclass of the entity's actual type and at least a <see cref="ModelEntity"/>
        /// </remarks>
        /// <typeparam name="THandle">The underlying type of the handle. Must be at least a <see cref="ModelEntity"/></typeparam>
        /// <param name="entity">The entity to obtain a handle of</param>
        /// <returns>The new handle to this referent</returns>
        /// <exception cref="InvalidStateException"></exception>
        public static EntityHandle<THandle> GetHandle<THandle>(this THandle entity) where THandle : ModelEntity
        {
            entity.GuardValidity();

            // If the entity's OwningTable wasn't aware of the entity, there was an error during the entity's creation
            // That should never be the case, so we throw an exception
            if (!entity.OwningTable.TryCreateHandle(entity, out EntityHandle<THandle>? handle))
                throw new InvalidStateException("Invalid state: the provided entity referenced a manager but was not tracked by the manager. Aborting.");

            return handle!;
        }

        /// <summary>Returns the number of valid handles</summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static int GetHandleCount(this ModelEntity entity)
        {
            entity.GuardValidity();
            return entity.OwningTable.GetHandleCount(entity);
        }

        /// <summary>
        /// Returns the most generic type a replacement entity can be to successfully transfer handles
        /// </summary>
        /// <remarks>
        /// This is equal to the most specified type of all the entity's handles
        /// </remarks>
        /// <param name="entity">The entity to query</param>
        /// <param name="maximalType">The most generic entity type the specified entity can transfer its handles to, or <see langword="null"/> if the manager did not manage the specified entity</param>
        /// <returns>
        /// <see langword="true"/> If the entity is managed by this manager<br/>
        /// <see langword="false"/> If the entity is not managed by this manager and its maximalType couldn't be found
        /// </returns>
        public static bool GetMaximalReplacementType(this ModelEntity entity, out Type? maximalType)
        {
            entity.GuardValidity();
            return entity.OwningTable.GetEntityMaximalReplacementType(entity, out maximalType);
        }

        /// <summary>
        /// Check if an entity can be replaced with another.
        /// </summary>
        /// <remarks>
        /// The new entity can only replace the old entity if all of its handles can safely be moved
        /// </remarks>
        /// <typeparam name="TNew"></typeparam>
        /// <typeparam name="TOld"></typeparam>
        /// <param name="oldEntity"></param>
        /// <param name="newEntity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidEntityUsedException"></exception>
        public static bool CanReplaceWith<TNew, TOld>(this TOld oldEntity, TNew newEntity)
            where TOld : ModelEntity where TNew : ModelEntity
        {
            oldEntity.GuardValidity();
            newEntity.GuardValidity();
            return oldEntity.OwningTable.CanReplaceEntityWith(oldEntity, newEntity);
        }

        /// <summary>
        /// Replace an entity with a new entity.
        /// </summary>
        /// <remarks>
        /// Both entities must be managed by this manager and they must be compatible<br/>
        /// <br/>
        /// Throws <see cref="IrreplaceableEntityException"/> if the entity being replaced is marked as irreplaceable<br/>
        /// Throws <see cref="IncompatibleEntityException"/> if the new entity is incompatible with at least one of the old entity's handles
        /// </remarks>
        /// <param name="oldEntity"></param>
        /// <param name="newEntity"></param>
        /// <returns>
        /// <see langword="true"/> if the entity was successfully replaced<br/>
        /// <see langword="false"/> if either the oldEntity or newEntity weren't managed by this manager
        /// </returns>
        /// <exception cref="IrreplaceableEntityException"></exception>
        /// <exception cref="IncompatibleEntityException"></exception>
        public static bool ReplaceWith<TNew, TOld>(this TNew oldEntity, TOld newEntity)
            where TOld : ModelEntity where TNew : ModelEntity
        {
            oldEntity.GuardValidity();
            newEntity.GuardValidity();
            return oldEntity.OwningTable.ReplaceEntity(oldEntity, newEntity);
        }
    }
}
