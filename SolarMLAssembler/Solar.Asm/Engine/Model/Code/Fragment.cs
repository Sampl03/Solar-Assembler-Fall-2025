using Solar.Asm.Engine.Model.Exceptions;
using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Code
{
    /// <summary>
    /// Fragments are the building blocks of sections which contain actual chunks.<br/>
    /// They allow sections to merge back together smoothly when spread across multiple sources
    /// or when non-contiguous within a single source
    /// </summary>
    /// <remarks>
    /// Fragments are fixed classes and cannot be extended. Subclass <see cref="Section"/> instead
    /// </remarks>
    public sealed class Fragment : CodeEntity
    {
        private readonly List<EntityHandle<Chunk>> _chunkHandles = [];
        public IEnumerable<Chunk> Chunks => _chunkHandles.Select(ch => ch.Ref!);

        // Section and PreviousFragment should only be set by the containing Section.
        internal EntityHandle<Section>? _section;
        internal EntityHandle<Fragment>? _prevFragment;
        public Section? Section => _section?.Ref;
        public Fragment? PreviousFragment => _prevFragment?.Ref;

        public Fragment() : base()
        {
        }

        /// <summary>
        /// Adds the chunk to the end of this chunk
        /// </summary>
        /// <param name="chunk"></param>
        public void AddChunk(Chunk chunk) => InsertChunk(_chunkHandles.Count, chunk);

        /// <summary>
        /// Adds the chunk at the specified location in this chunk
        /// </summary>
        /// <param name="i"></param>
        /// <param name="chunk"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="CannotAddException"></exception>
        public void InsertChunk(int i, Chunk chunk)
        {
            GuardValidity();

            if (i < 0 || i > _chunkHandles.Count)
                throw new IndexOutOfRangeException();

            if (chunk.Fragment is not null)
                throw new CannotAddException("Fragment could not add Chunk which already belongs to another section");

            if (_chunkHandles.Where(ch => ReferenceEquals(ch.Ref, chunk)).Any()) // Ensure uniqueness
                throw new CannotAddException("Fragment could not add Chunk which already belongs to it");

            // Insert Chunk at the specified location
            chunk._fragment = this.GetHandle();
            if (i == _chunkHandles.Count)
                _chunkHandles.Add(chunk.GetHandle());
            else
                _chunkHandles.Insert(i, chunk.GetHandle());

            /* Update the links */
            EntityHandle<Chunk>?
                prevChunkHandle = null,
                nextChunkHandle = _chunkHandles.ElementAtOrDefault(i + 1);

            // If there's a next handle, we need to point it to the new chunk and borrow its prev handle
            if (nextChunkHandle is not null)
            {
                prevChunkHandle = nextChunkHandle.Ref!._prevChunk;
                nextChunkHandle.Ref!._prevChunk = chunk.GetHandle();
            }
            // Otherwise we need to create our own prev handle
            else
            {
                prevChunkHandle = _chunkHandles.ElementAtOrDefault(i - 1)?.Ref!.GetHandle();
            }

            // Assign prevChunkHandle to the new chunk
            chunk._prevChunk?.Dispose(); // Dispose of any previous chunk reference if it exists
            chunk._prevChunk = prevChunkHandle;

            // Ensure the chunk is now valid
            chunk.GuardValidity();
        }

        /// <summary>
        /// Remove the specified chunk if it is located within this fragment
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns>
        /// <see langword="true"/> if the chunk was removed<br/>
        /// <see langword="false"/> if the chunk is not within this fragment
        /// </returns>
        public bool RemoveChunk(Chunk chunk)
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
                nextChunkHandle.Ref!._prevChunk!.Dispose();
                nextChunkHandle.Ref!._prevChunk = chunk._prevChunk;
                chunk._prevChunk = null;
            }

            // Remove the chunk
            var chunkHandle = _chunkHandles[chunkId];
            _chunkHandles.RemoveAt(chunkId);

            // Remove its reference to the parent fragment
            chunkHandle.Ref!._fragment!.Dispose();
            chunkHandle.Ref!._fragment = null;

            // Remove its reference to the previous chunk if not already transferred to the next
            chunkHandle.Ref!._prevChunk?.Dispose();
            chunkHandle.Ref!._prevChunk = null;

            // Dispose of the fragment's handle
            chunkHandle.Dispose();

            return true;
        }

        /// <param name="chunk"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="chunk"/> is stored in this fragment<br/>
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Contains(Chunk chunk) => IndexOf(chunk) != -1;

        /// <param name="chunk"></param>
        /// <returns>
        /// The index of the unique handle to <paramref name="chunk"/> in this fragment,
        /// or <see langword="-1"/> if not located in this fragment
        /// </returns>
        public int IndexOf(Chunk chunk)
        {
            GuardValidity();

            // Ensure the chunk is valid
            chunk.GuardValidity();

            // Get the handle that corresponds, if it exists
            var chunkHandle = _chunkHandles.Where(ch => ReferenceEquals(ch.Ref, chunk)).SingleOrDefault();
            if (chunkHandle is null)
                return -1;

            return _chunkHandles.IndexOf(chunkHandle);
        }


        protected override bool OnValidityGuard()
        {
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
                if (chunk.PreviousChunk! != _chunkHandles[i - 1].Ref!)
                    return false;

                // Check that every chunk points to this section
                if (chunk.Fragment! != this)
                    return false;

            }

            return true;
        }

        public override IReadOnlyList<byte> EmitBytes()
        {
            GuardValidity();

            List<byte> result = [];

            foreach (Chunk chunk in _chunkHandles.Select(ch => ch.Ref!))
                result.AddRange(chunk.EmitBytes());

            return result;
        }

        public override BinaryPatch[] EmitPatches()
        {
            GuardValidity();

            var patches = new List<BinaryPatch>();

            foreach (Chunk chunk in _chunkHandles.Select(fh => fh.Ref!))
            {
                var chunkPatches = chunk.EmitPatches();

                // Adjust the cell offsets of each patch to account for the chunk's position within the fragment
                ulong chunkOffset = chunk.CalculateMemCellOffset();
                for (int i = 0; i < chunkPatches.Length; i++)
                    chunkPatches[i].CellOffset += chunkOffset;

                // Append the chunk's updated patches to the fragments's patches
                patches.AddRange(chunkPatches);
            }

            return [.. patches];
        }

        public override ulong CalculateMemCellOffset()
        {
            GuardValidity();

            if (PreviousFragment is null)
                return 0;

            // Address of this fragment is the address of the previous segment plus its size
            return PreviousFragment!.CalculateMemCellOffset() + PreviousFragment!.CalculateMemSize();
        }

        public override ulong CalculateMemCellVirtualAddress()
        {
            return Section!.CalculateMemCellVirtualAddress() + CalculateMemCellOffset();
        }

        protected override void OnInvalidated()
        {
            _section?.Dispose();
            _section = null;

            _prevFragment?.Dispose();
            _prevFragment = null;

            foreach (var chunkHandle in _chunkHandles)
                chunkHandle.Dispose();

            _chunkHandles.Clear();
        }
    }
}
