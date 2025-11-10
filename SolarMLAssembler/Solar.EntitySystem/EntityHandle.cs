using Solar.EntitySystem.Exceptions;

namespace Solar.EntitySystem
{
    /// <summary>
    /// Parent class of EntityHandle to allow non-generic handling of handles.
    /// </summary>
    public abstract class EntityHandleBase : IDisposable
    {
        /// <summary>Returns the most generic type this EntityHandle can contain</summary>
        public abstract Type EntityMaxType { get; }

        /// <summary>Returns the referent as an untyped <see cref="ModelEntity"/>, or null if the handle is invalid</summary>
        public abstract ModelEntity? UntypedRef { get; }

        /// <summary>Returns true if the handle has not been invalidated.</summary>
        public abstract bool IsValid { get; }

        /// <summary>
        /// Internal function to change the referent of this handle
        /// </summary>
        /// <param name="newReferent"></param>
        internal abstract void ReplaceReferent(ModelEntity newReferent);

        public abstract void Dispose();
    }

    /// <summary>
    /// An opaque handle to an entity compatible with the specified type.
    /// </summary>
    /// <typeparam name="TEntity">The most generic type of entity this handle can refer to</typeparam>
    public sealed class EntityHandle<TEntity> : EntityHandleBase where TEntity : ModelEntity
    {
        public override Type EntityMaxType => typeof(TEntity);

        public override ModelEntity? UntypedRef => Ref;

        public override bool IsValid => Ref is not null;

        /// <summary>Returns the referent, or null if the handle is invalid.</summary>
        public TEntity? Ref { get; private set; }

        /// <summary>
        /// Invalidates the handle by calling the manager
        /// </summary>
        /// <remarks>
        /// Throws <see cref="InvalidStateException"/> if the handle's referent's manager was not tracking the referent
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if the handle was successfully invalidated,<br/>
        /// <see langword="false"/> if it was already invalid.
        /// </returns>
        /// <exception cref="InvalidStateException"/>
        private bool Dispose(bool disposing)
        {
            // If already disposed, do nothing
            if (Ref is null)
                return false;

            // Call the manager to remove this handle from the referent's incoming handle list
            // - If an InvalidStateException is thrown, something went wrong so we don't catch it
            // - If the function returns false, then the referent's OwningTable wasn't aware of it. This is an invalid state and we throw.
            if (!Ref.OwningTable!.RemoveEntityHandle(this))
                throw new InvalidStateException("Invalid state: the handle's referent entity referenced a manager but was not tracked by the manager. Aborting.");

            // The handle has now been removed, so we set the reference to null.
            // It should get deleted once all other strong references to it have been removed.
            Ref = null;

            return true;
        }

        /// <summary>
        /// Internal function to move a handle from an old entity to the new entity replacing it
        /// </summary>
        /// <remarks>
        /// Should only be called by the <see cref="ModelEntity"/> extension <see cref="EntityExtensions.GetHandle{THandle, TEntity}(TEntity)"/><br/>
        /// </remarks>
        /// <typeparamref name="TNew"/> should be a subclass of <typeparamref name="TEntity"/> so that the handle remains valid
        /// </remarks>
        /// <param name="newReferent">The new entity to point the handle to</param>
        internal void ReplaceReferent<TNew>(TNew newReferent) where TNew : TEntity
        {
            Ref = newReferent;
        }

        internal override void ReplaceReferent(ModelEntity newReferent)
        {
            if (newReferent is not TEntity)
                throw new IncompatibleEntityException(
                    $"Entity of type '{newReferent.GetType().FullName}' is not assignable to handle's maximal entity type '{typeof(TEntity).FullName}'",
                    typeof(TEntity), newReferent.GetType()
                );

            Ref = (TEntity)newReferent;
        }

        /// <summary>
        /// Construct a new EntityHandle pointing to the referent.
        /// For use by HandleFactory only
        /// </summary>
        /// <param name="referent">The entity to be refered to by this handle.</param>
        internal EntityHandle(TEntity referent)
        {
            Ref = referent;       
        }

        /// <summary>
        /// Invalidate the handle, removing it from the referent's incoming handle list
        /// </summary>
        public override void Dispose()
        {
            if (Dispose(disposing: true))
                GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Last resort: if the handle is garbage-collected without being invalidated, invalidate it now.
        /// </summary>
        ~EntityHandle()
        {
            Dispose(disposing: false);
        }
    }
}