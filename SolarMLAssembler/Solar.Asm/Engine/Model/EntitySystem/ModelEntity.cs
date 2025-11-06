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
        public EntityManager OwningTable { get; internal set; }

        public EntityState State { get; private set; } = EntityState.Uninitialised;

        public bool IsValid => State == EntityState.Valid;

        protected ModelEntity(EntityManager owningTable)
        {
            OwningTable = owningTable;
        }

        /// <summary>
        /// Throws an exception or returns a boolean if the entity is invalid.<br/>
        /// <br/>
        /// An entity may enter an invalid state, such as when it has been replaced by another entity.<br/>
        /// Improperly structured code may still hold direct references to invalid entities, on which it may try to call public functions.<br/>
        /// <br/>
        /// It is the responsibility of the derived or extension class to call this method at the start of all public methods to handle this scenario appropriately.
        /// </summary>
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
            if (!IsValid && throwIfInvalid)
                throw new InvalidEntityUsedException("This entity is invalid and can no longer be used.");
            return State != EntityState.Valid;
        }

        /// <summary>
        /// Initialises an entity, must be called before an entity is used
        /// </summary>
        /// <remarks>
        /// Throws <see cref="InvalidEntityUsedException"/> if this is a <see cref="IUniqueEntity"/>
        /// and an equivalent entity already exists.
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if initialised successfully<br/>
        /// <see langword="false"/> if not in an uninitialised state
        /// </returns>
        /// <exception cref="InvalidEntityUsedException"></exception>
        public bool Initialise()
        {
            if (State != EntityState.Uninitialised)
                return false;

            State = EntityState.Valid;
            OwningTable.RegisterEntity(this); // this may invalidate the entity if it's IUniqueEntity
            GuardValidity(); // in which case this will throw an exception
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

            // Notify the owning table. It is in charge of verifying that there are no more active handles
            OwningTable.UnregisterEntity(this);

            // If code reaches here, the entity was successfully removed and we can mark it invalid
            State = EntityState.Invalid;

            return true;
        }
    }
}