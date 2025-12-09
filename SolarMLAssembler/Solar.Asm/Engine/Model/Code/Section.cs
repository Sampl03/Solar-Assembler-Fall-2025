using Solar.Asm.Engine.Model.Exceptions;
using Solar.EntitySystem;
using Solar.EntitySystem.Behavior;
using Solar.EntitySystem.Exceptions;

namespace Solar.Asm.Engine.Model.Code
{
    /// <summary>
    /// Sections are the base relocatable unit for code. They contain <see cref="Fragment"/> objects and may merge<br/>
    /// Sections are merged if<br/>
    /// - the names are equal;<br/>
    /// - the <see cref="Flags"/>'s IsMergeable flag is <see langword="true"/> for both sections<br/>
    /// - the <see cref="Flags"/> values are compatible.
    /// </summary>
    /// <param name="name">The name of the section, acting as its identifier</param>
    /// <param name="flags">The section's flags</param>
    public class Section(string name, SectionFlags flags) : CodeEntity(), IUniqueEntity
    {
        private static uint _nextDeclaration = 0;

        private readonly List<EntityHandle<Fragment>> _fragmentHandles = [];
        public IEnumerable<Fragment> Fragments => _fragmentHandles.Select(fh => fh.Ref!);

        public string Name { get; init; } = name;

        /// <summary>
        /// The declaration order of this section.
        /// </summary>
        /// <remarks>
        /// This is automatically set to an increasing counter and should not be overriden
        /// </remarks>
        public uint DeclarationOrder { get; } = _nextDeclaration++;

        /// <summary>
        /// The desired address of this section, in the units of the target architecture
        /// </summary>
        public ulong DesiredAddress { get; set; } = 0;

        /// <summary>
        /// Determines whether <see cref="DesiredAddress"/> is a fixed value or not<br/>
        /// If <see langword="true"/>, formatters must ensure this address, and such sections with incompatible addresses must be discarded.
        /// </summary>
        public bool IsDesiredAddressFixed { get; set; } = false;

        public SectionFlags Flags { get; init; } = flags;
        
        /// <summary>
        /// Adds the fragment to the end of this section
        /// </summary>
        /// <param name="fragment"></param>
        public virtual void AddFragment(Fragment fragment) => InsertFragment(_fragmentHandles.Count, fragment);

        /// <summary>
        /// Adds the fragment at the specified location in this section
        /// </summary>
        /// <param name="i"></param>
        /// <param name="fragment"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="CannotAddException"></exception>
        public virtual void InsertFragment(int i, Fragment fragment)
        {
            GuardValidity();

            if (i < 0 || i > _fragmentHandles.Count)
                throw new IndexOutOfRangeException();

            if (fragment.Section is not null)
                throw new CannotAddException("Section could not add Fragment which already belongs to another section");

            if (_fragmentHandles.Where(fh => ReferenceEquals(fh.Ref, fragment)).Any()) // Ensure uniqueness
                throw new CannotAddException("Section could not add Fragment which already belong to it");

            // Insert Fragment at the specified location
            if (i == _fragmentHandles.Count)
                _fragmentHandles.Add(fragment.GetHandle());
            else
                _fragmentHandles.Insert(i, fragment.GetHandle());
            fragment._section = this.GetHandle();

            /* Update the links */
            EntityHandle<Fragment>?
                prevFragmentHandle = null,
                nextFragmentHandle = _fragmentHandles.ElementAtOrDefault(i + 1);

            // If there's a next handle, we need to point it to new fragment and borrow its prev handle
            if (nextFragmentHandle is not null)
            {
                prevFragmentHandle = nextFragmentHandle.Ref!._prevFragment;
                nextFragmentHandle.Ref!._prevFragment = fragment.GetHandle();
            }
            // Otherwise we need to create our own prev handle
            else
            {
                prevFragmentHandle = _fragmentHandles.ElementAtOrDefault(i - 1)?.Ref!.GetHandle();
            }
            
            // Assign prevFragmentHandle to the new fragment
            fragment._prevFragment = prevFragmentHandle;

            // Ensure the fragment is now valid
            fragment.GuardValidity();
        }

        /// <summary>
        /// RemoveChunk the specified fragment if it is located within this section
        /// </summary>
        /// <param name="fragment"></param>
        /// <returns>
        /// <see langword="true"/> if the fragment was removed<br/>
        /// <see langword="false"/> if the fragment is not within this section
        /// </returns>
        public virtual bool RemoveFragment(Fragment fragment)
        {
            GuardValidity();

            // Ensure the fragment is valid
            fragment.GuardValidity();

            // Fetch the handles to delete, if any
            var fragId = IndexOf(fragment);
            if (fragId == -1)
                return false;

            /* Update the links */
            var nextFragmentHandle = _fragmentHandles.ElementAtOrDefault(fragId + 1);

            // If there's a next node, we need to update its prev reference
            if (nextFragmentHandle is not null) 
            {
                nextFragmentHandle.Ref!._prevFragment!.Dispose();
                nextFragmentHandle.Ref!._prevFragment = fragment._prevFragment;
                fragment._prevFragment = null;
            }

            // RemoveChunk the fragment
            var fragHandle = _fragmentHandles[fragId];
            _fragmentHandles.RemoveAt(fragId);

            // RemoveChunk its reference to the parent section
            fragHandle.Ref!._section!.Dispose();
            fragHandle.Ref!._section = null;

            // RemoveChunk its reference to the previous fragment if not already transferred to the next
            fragHandle.Ref!._prevFragment?.Dispose();
            fragHandle.Ref!._prevFragment = null;

            // Dispose of the section's handle
            fragHandle.Dispose();

            return true;
        }

        /// <param name="fragment"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="fragment"/> is stored in this section<br/>
        /// <see langword="false"/> otherwise.
        /// </returns>
        public virtual bool Contains(Fragment fragment) => IndexOf(fragment) != -1;

        /// <param name="fragment"></param>
        /// <returns>
        /// The index of the unique handle to <paramref name="fragment"/> in this section,
        /// or <see langword="-1"/> if not located in this section
        /// </returns>
        public virtual int IndexOf(Fragment fragment)
        {
            GuardValidity();

            // Ensure the fragment is valid
            fragment.GuardValidity();

            // Get the handle that corresponds, if it exists
            var fragHandle = _fragmentHandles.Where(fh => ReferenceEquals(fh.Ref, fragment)).SingleOrDefault();
            if (fragHandle is null)
                return -1;

            return _fragmentHandles.IndexOf(fragHandle);
        }

        protected override bool OnValidityGuard()
        {
            /* We need to check that all the links managed by this section are valid */

            // If there are no fragments, it's fine
            if (_fragmentHandles.Count == 0)
                return true;

            // Otherwise, check that the first fragment has no PreviousFragment handle
            if (_fragmentHandles[0].Ref!.PreviousFragment is not null)
                return false;

            // Then, we check every fragment
            for (int i = 1; i < _fragmentHandles.Count; i++)
            {
                Fragment fragment = _fragmentHandles[i].Ref!;

                // Check that PreviousFragment is indeed the correct fragment
                if (fragment.PreviousFragment! != _fragmentHandles[i - 1].Ref!)
                    return false;

                // Check that every fragment points to this section
                if (fragment.Section! != this)
                    return false;

            }

            return true;
        }

        public virtual bool CanMergeInto(IMergeable destination)
        {
            GuardValidity();
            if (destination is not Section)
                return false;

            var destSection = (Section)destination;

            if (Name != destSection.Name)
                return false;

            switch(IsDesiredAddressFixed, destSection.IsDesiredAddressFixed)
            {
                case (true, true): // If both are fixed, their addresses must agree
                    if (DesiredAddress != destSection.DesiredAddress)
                        return false;
                    break;
                case (true, false): // incoming can't be fixed otherwise since merging would change its address
                    return false;
                default: // Otherwise we can merge the incoming section
                    break;
            }

            return Flags.CanMergeInto(destSection.Flags);
        }

        public virtual void MergeInto(IMergeable destination)
        {
            if (!CanMergeInto(destination))
                throw new CannotMergeException($"Could not merge Section into entity of type {destination.GetType().FullName}", this, destination);

            var destSection = (Section)destination;

            // To ensure that we don't break arbitrary rules the destination section may have,
            // we need to transfer our fragments one by one via our RemoveFragment() and the destination's AddFragment()
            while (_fragmentHandles.Count > 0)
            {
                // Hold a temporary reference to the fragment
                Fragment fragment = _fragmentHandles[0].Ref!;

                // Open a new temporary handle to avoid deletion, which automatically gets closed at the current loop's end
                using var tempFragHandle = fragment.GetHandle();

                // Remove the fragment from this Section
                RemoveFragment(fragment);

                // Add it to the new section
                destSection.AddFragment(fragment);
            }
        }

        public virtual bool EntityEquivalent(ModelEntity other)
        {
            if (other is not Section)
                return false;

            var otherSection = (Section)other;

            if (Name != otherSection.Name)
                return false;

            return Flags.CanMergeInto(otherSection.Flags);
        }

        public virtual int EntityHash()
        {
            return Name.GetHashCode();
        }

        public override IReadOnlyList<byte> EmitBytes()
        {
            GuardValidity();

            List<byte> result = [];

            foreach (Fragment frag in _fragmentHandles.Select(fh => fh.Ref!))
                result.AddRange(frag.EmitBytes());

            return result;
        }

        public override BinaryPatch[] EmitPatches()
        {
            GuardValidity();

            var patches = new List<BinaryPatch>();

            foreach (Fragment frag in _fragmentHandles.Select(fh => fh.Ref!))
            {
                var fragPatches = frag.EmitPatches();

                // Adjust the cell offsets of each patch to account for the fragment's position within the section
                ulong fragOffset = frag.CalculateMemCellOffset();
                for (int i = 0; i < fragPatches.Length; i++)
                {
                    fragPatches[i].CellOffset += fragOffset;
                }

                // Append the fragment's patches to the section's patches
                patches.AddRange(fragPatches);
            }

            return [.. patches];
        }

        public override ulong CalculateMemCellOffset()
        {
            GuardValidity();

            return 0L;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// <b>Note:</b> This is always equal to <see cref="DesiredAddress"/> for sections
        /// </remarks>
        public sealed override ulong CalculateMemCellVirtualAddress()
        {
            GuardValidity();

            return DesiredAddress;
        }

        protected override void OnInvalidated()
        {
            foreach (var fragHandle in _fragmentHandles)
                fragHandle.Dispose();

            _fragmentHandles.Clear();
        }
    }
}
