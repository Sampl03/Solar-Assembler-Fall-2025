using Solar.EntitySystem.Behavior;
using Solar.EntitySystem.Exceptions;

namespace Solar.EntitySystem
{
    public enum EntityState
    {
        Uninitialised,
        Valid,
        Invalid
    }

    /// <summary>
    /// Represents an entity in the model which can be accessed via an opaque handle
    /// </summary>
    public abstract class ModelEntity
    {
        public EntityManager? OwningTable { get; internal set; } = null;

        public EntityState State { get; private set; } = EntityState.Uninitialised;

        public bool IsValid => State == EntityState.Valid;

        protected ModelEntity() { }

        /// <summary>
        /// Throws an exception or returns a boolean if the entity is invalid.<br/>
        /// <br/>
        /// An entity may enter an invalid state, such as when it has been replaced by another entity.<br/>
        /// Improperly structured code may still hold direct references to invalid entities, on which it may try to call public functions.<br/>
        /// <br/>
        /// It is the responsibility of the derived or extension class to call this method at the start of all public methods to handle this scenario appropriately.
        /// </summary>
        /// 
        /// <remarks>
        /// This will also call <seealso cref="OnValidityGuard(bool)"/>, which can be overridden to add custom checks
        /// </remarks>
        /// 
        /// <param name="throwIfInvalid">
        /// If <see langword="true"/> and the entity is invalid, an <see cref="InvalidEntityUsedException"/> will be thrown.<br/>
        /// Otherwise, the method will return whether or not the entity is invalid.
        /// </param>
        /// 
        /// <returns>
        /// If <paramref name="throwIfInvalid"/> is <see langword="false"/>, this method returns whether or not the entity is invalid<br/>
        /// See <see cref="IsValid"/>
        /// </returns>
        /// 
        /// <exception cref="InvalidEntityUsedException">Thrown if the entity is invalid and <paramref name="throwIfInvalid"/> is <see langword="true"/>.</exception>
        public bool GuardValidity(bool throwIfInvalid = true)
        {
            bool isInValidState;
            Exception? caughtError = null;
            try
            {
                isInValidState = IsValid && OnValidityGuard();
            }
            catch (Exception ex)
            {
                isInValidState = false;
                caughtError = ex;
            }

            if (!isInValidState)
            {
                if (caughtError is not null)
                    throw new InvalidEntityUsedException("This entity threw an exception during state validation", caughtError);
                else
                    throw new InvalidEntityUsedException("This entity is invalid and can no longer be used.");
            }
            
            return true;
        }

        /// <summary>
        /// Hook method called by <seealso cref="GuardValidity(bool)"/>.<br/>
        /// Can be overriden by derived classes to add validation behaviour.
        /// </summary>
        /// <returns>
        /// If <paramref name="throwIfInvalid"/> is <see langword="false"/>, this method returns whether or not the entity is in a usable state<br/>
        /// </returns>
        protected virtual bool OnValidityGuard() => true;

        /// <summary>
        /// Initialises an entity and registers it with a manager, must be called before an entity is used
        /// </summary>
        /// <remarks>
        /// Throws <see cref="UniquenessConstraintFailedException"/> if the entity is <see cref="IUniqueEntity"/> and another duplicate already exists within the same manager (incorrect initialisation)<br/>
        ///  -> Use <see cref="UniqueEntity.Make{TUnique}(TUnique)"/> instead.
        /// </remarks>
        /// <param name="owningTable">The manager to register this entity with</param>
        /// <returns>
        /// <see langword="true"/> if initialised successfully<br/>
        /// <see langword="false"/> if not in an uninitialised state
        /// </returns>
        /// <exception cref="UniquenessConstraintFailedException"></exception>
        public virtual bool Initialise(EntityManager owningTable)
        {
            if (State != EntityState.Uninitialised)
                return false;

            State = EntityState.Valid;
            owningTable.RegisterEntity(this);
            return true;
        }

        /// <summary>
        /// Invalidates this entity and removes it from the model.
        /// </summary>
        /// <remarks>
        /// Throws <see cref="CannotRemoveException"/> if there are still active handles pointing to this entity."/>
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if the entity was successfully removed<br/>
        /// <see langword="false"/> if it was already invalidated
        /// </returns>
        /// <exception cref="CannotRemoveException"></exception>
        protected internal bool Invalidate()
        {
            // Return false if the entity is already invalid
            if (!IsValid)
                return false;

            // Let subclasses clean up
            OnInvalidated();

            // Notify the owning table. It is in charge of verifying that there are no more active handles
            OwningTable!.UnregisterEntity(this);

            // If code reaches here, the entity was successfully removed and we can mark it invalid
            State = EntityState.Invalid;

            return true;
        }

        protected virtual void OnInvalidated() { }
    }
}