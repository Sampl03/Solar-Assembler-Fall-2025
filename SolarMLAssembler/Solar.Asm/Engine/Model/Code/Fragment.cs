using Solar.Asm.Engine.Model.Exceptions;
using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Code
{
    /// <summary>
    /// Fragments are the building blocks of sections which contain actual chunks.<br/>
    /// They allow sections to merge back together smoothly when spread across multiple sources
    /// or when non-contiguous within a single source
    /// </summary>
    public class Fragment : CodeEntity
    {
        private readonly IList<EntityHandle<Chunk>> _chunkHandles = [];

        public IReadOnlyList<EntityHandle<Chunk>> Chunks { get => _chunkHandles.AsReadOnly(); }

        // Section and PreviousFragment should only be set by the containing Section.
        public EntityHandle<Section>? Section { get; internal set; }
        public EntityHandle<Fragment>? PreviousFragment { get; internal set; }

        public Fragment() : base()
        {
        }

        /// <summary>
        /// Adds the chunk to the end of this chunk
        /// </summary>
        /// <param name="chunk"></param>
        public virtual void AddChunk(Chunk chunk) => InsertChunk(_chunkHandles.Count, chunk);

        /// <summary>
        /// Adds the chunk at the specified location in this chunk
        /// </summary>
        /// <param name="i"></param>
        /// <param name="chunk"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="SmlaCannotAddException"></exception>
        public virtual void InsertChunk(int i, Chunk chunk)
        {
            GuardValidity();

            if (i < 0 || i > _chunkHandles.Count)
                throw new IndexOutOfRangeException();

            if (chunk.Fragment is not null)
                throw new SmlaCannotAddException("Fragment could not add Chunk which already belongs to another section");

            if (_chunkHandles.Where(ch => ch.Ref == chunk).Any()) // Ensure uniqueness
                throw new SmlaCannotAddException("Fragment could not add Chunk which already belongs to it");

            // Insert Chunk at the specified location
            if (i == _chunkHandles.Count)
                _chunkHandles.Add(chunk.GetHandle());
            else
                _chunkHandles.Insert(i, chunk.GetHandle());
            chunk.Fragment = this.GetHandle();

            /* Update the links */
            EntityHandle<Chunk>?
                prevChunkHandle = null,
                nextChunkHandle = _chunkHandles.ElementAtOrDefault(i + 1);

            // If there's a next handle, we need to point it to the new chunk and borrow its prev handle
            if (nextChunkHandle is not null)
            {
                prevChunkHandle = nextChunkHandle.Ref!.PreviousChunk;
                nextChunkHandle.Ref!.PreviousChunk = chunk.GetHandle();
            }
            // Otherwise we need to create our own prev handle
            else
            {
                prevChunkHandle = _chunkHandles.ElementAtOrDefault(i - 1)?.Ref!.GetHandle();
            }

            // Assign prevChunkHandle to the new chunk
            chunk.PreviousChunk?.Dispose(); // Dispose of any previous chunk reference if it exists
            chunk.PreviousChunk = prevChunkHandle;

            // Ensure the chunk is now valid
            chunk.GuardValidity();
            RequireRecalculation();
        }

        /// <summary>
        /// Remove the specified chunk if it is located within this fragment
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns>
        /// <see langword="true"/> if the chunk was removed<br/>
        /// <see langword="false"/> if the chunk is not within this fragment
        /// </returns>
        public virtual bool RemoveChunk(Chunk chunk)
        {
            GuardValidity();

            // Ensure the chunk is valid
            chunk.GuardValidity();

            // Fetch the handles to delete, if any
            var chunkId = IndexOf(chunk);
            if (chunkId == -1)
                return false;

            /* Update the links */
            var nextChunkHandle = _chunkHandles.ElementAtOrDefault(chunkId + 1);

            // If there's a next node, we need to update its prev reference
            if (nextChunkHandle is not null)
            {
                nextChunkHandle.Ref!.PreviousChunk!.Dispose();
                nextChunkHandle.Ref!.PreviousChunk = chunk.PreviousChunk;
                chunk.PreviousChunk = null;
            }

            // Remove the chunk
            var chunkHandle = _chunkHandles[chunkId];
            _chunkHandles.RemoveAt(chunkId);

            // Remove its reference to the parent fragment
            chunkHandle.Ref!.Fragment!.Dispose();
            chunkHandle.Ref!.Fragment = null;

            // Remove its reference to the previous chunk if not already transferred to the next
            chunkHandle.Ref!.PreviousChunk?.Dispose();
            chunkHandle.Ref!.PreviousChunk = null;

            // Dispose of the fragment's handle
            chunkHandle.Dispose();
            RequireRecalculation();

            return true;
        }

        /// <param name="chunk"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="chunk"/> is stored in this fragment<br/>
        /// <see langword="false"/> otherwise.
        /// </returns>
        public virtual bool Contains(Chunk chunk) => IndexOf(chunk) != -1;

        /// <param name="chunk"></param>
        /// <returns>
        /// The index of the unique handle to <paramref name="chunk"/> in this fragment,
        /// or <see langword="-1"/> if not located in this fragment
        /// </returns>
        public virtual int IndexOf(Chunk chunk)
        {
            GuardValidity();

            // Ensure the chunk is valid
            chunk.GuardValidity();

            // Get the handle that corresponds, if it exists
            var fragHandle = _chunkHandles.Where(fh => fh.Ref == chunk).SingleOrDefault();
            if (fragHandle is null)
                return -1;

            return _chunkHandles.IndexOf(fragHandle);
        }


        protected override bool OnValidityGuard()
        {
            // If the section is null, then this fragment is unassigned and cannot be used
            if (Section is null)
                return false;

            /* We need to check that all the links managed by this fragment are valid */

            // If there are no chunks, it's fine
            if (_chunkHandles.Count == 0)
                return true;

            // Otherwise, check that the first chunk has no PreviousChunk handle
            if (_chunkHandles[0].Ref!.PreviousChunk is not null)
                return false;

            // Then, we check every chunk
            for (int i = 1; i < _chunkHandles.Count; i++)
            {
                Chunk chunk = _chunkHandles[i].Ref!;

                // Check that PreviousChunk is indeed the correct chunk
                if (chunk.PreviousChunk!.Ref! != _chunkHandles[i - 1].Ref!)
                    return false;

                // Check that every chunk points to this section
                if (chunk.Fragment!.Ref! != this)
                    return false;

            }

            return true;
        }

        private bool _stateChanged = true;
        public override bool NeedsRecalculation()
        {
            GuardValidity();

            // If instance state changed, we need to recalculate
            if (_stateChanged)
                return true;

            // Otherwise, we check if any of the chunks changed
            _stateChanged = _chunkHandles.Any(ch => ch.Ref!.NeedsRecalculation());
            return _stateChanged;
        }

        public override void RequireRecalculation()
        {
            GuardValidity();

            if (!_stateChanged)
            {
                Section!.Ref!.RequireRecalculation(); // Notify the parent if we didn't already do so before
                _stateChanged = true;
            }

        }

        private readonly List<byte> _cachedBytes = [];
        public override IReadOnlyList<byte> EmitBytes()
        {
            GuardValidity();

            // If we don't need a recalculation, just used the cached value
            if (!NeedsRecalculation())
                return _cachedBytes;

            // Otherwise, we reset the cache and get the bytes of each chunk
            _cachedBytes.Clear();

            // For each chunk,
            foreach (Chunk chunk in _chunkHandles.Select(ch => ch.Ref!))
            {
                // Fetch its byte representation
                var nextBytes = chunk.EmitBytes();

                // Append it to the cache
                _cachedBytes.AddRange(nextBytes);
            }

            _cachedBytes.TrimExcess();
            return _cachedBytes;
        }

        /// <returns>The offset in bytes of this fragment from the start of its containing section</returns>
        public override long CalculateByteOffset()
        {
            GuardValidity();

            if (PreviousFragment is null)
                return 0;

            // Address of this fragment is the address of the previous segment plus its size
            return PreviousFragment.Ref!.CalculateByteOffset() + PreviousFragment.Ref!.CalculateByteSize();
        }
    }
}
