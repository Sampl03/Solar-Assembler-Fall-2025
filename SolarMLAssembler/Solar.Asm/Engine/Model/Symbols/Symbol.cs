using Solar.Asm.Engine.Model.Code;
using Solar.Asm.Engine.Model.Exceptions;
using Solar.EntitySystem;
using Solar.EntitySystem.Exceptions;
using Solar.EntitySystem.Behavior;

namespace Solar.Asm.Engine.Model.Symbols
{
    /// <summary>
    /// Sealed class that represents a Symbol.
    /// </summary>
    public sealed class Symbol(QualifiedName name) : ModelEntity(), IUniqueEntity
    {
        /// <summary>
        /// The Program in which this symbol was declared
        /// </summary>
        public Program OwningProgram
        {
            get
            {
                GuardValidity();
                return (Program)OwningTable!.Context;
            }
        }

        #region Main Properties

        /// <summary>
        /// The fully qualified name of this symbol
        /// </summary>
        public QualifiedName FullyQualifiedName => name;

        /// <summary>
        /// The type of target this symbol points to
        /// </summary>
        public SymbolTarget Target { get; private set; } = SymbolTarget.UNDEFINED;

        /// <summary>
        /// The binding type of this symbol
        /// </summary>
        public SymbolBindType BindingType
        { 
            get => _bindingType; 
            set
            {
                if (!Enum.IsDefined(typeof(SymbolBindType), value))
                    throw new InvalidSymbolException($"Tried assigning invalid symbol binding type: {value} to a symbol", this);
                _bindingType = value;
            }
        }
        private SymbolBindType _bindingType = SymbolBindType.LOCAL;

        #endregion

        #region Target Specific Properties

        /// <summary>
        /// The absolute value of this symbol, if it is <see cref="SymbolTarget.ABSOLUTE"/>
        /// </summary>
        public ulong AbsoluteValue { get; private set; } = 0UL;

        /// <summary>
        /// The section this symbol references, if it is <see cref="SymbolTarget.SECTION"/>. Otherwise should be null
        /// </summary>
        public EntityHandle<Section>? TargetSection
        {
            get => _targetSection;
            set { _targetSection?.Dispose(); _targetSection = value; }
        }
        private EntityHandle<Section>? _targetSection;

        /// <summary>
        /// The chunk this symbol attaches to, if it is <see cref="SymbolTarget.LABEL"/>. Otherwise should be null
        /// </summary>
        public EntityHandle<Chunk>? TargetChunk
        {
            get => _targetChunk;
            set { _targetChunk?.Dispose(); _targetChunk = value; }
        }
        private EntityHandle<Chunk>? _targetChunk = null;

        /// <summary>
        /// The offset of this symbol from the chunk it attaches to, in memory cells, if it is <see cref="SymbolTarget"/>.
        /// </summary>
        public ulong MemCellOffset { get; set; } = 0;

        /// <returns>
        /// The value of this symbol. If the symbol is undefined, this returns  <see langword="0L"/>
        /// </returns>
        /// <exception cref="InvalidSymbolException"></exception>
        public ulong GetValue()
        {
            GuardValidity();

            // Label symbol's value is section address + fragment offset + chunk offset + mem cell offset

            return Target switch
            {
                SymbolTarget.ABSOLUTE
                    => AbsoluteValue,
                SymbolTarget.SECTION
                    => TargetSection?.Ref!.CalculateMemCellVirtualAddress() ??
                        throw new InvalidSymbolException("Tried getting value of section symbol with null target section", this),
                SymbolTarget.LABEL
                    => (TargetChunk?.Ref!.CalculateMemCellVirtualAddress() + MemCellOffset) ?? 
                        throw new InvalidSymbolException("Tried getting value of label symbol with null target chunk", this),
                SymbolTarget.UNDEFINED
                    => 0L, // Undefined symbols default to 0

                _ => throw new InvalidSymbolException("Tried getting value of symbol with invalid target type", this),
            };
        }

        #endregion

        #region Attribute Management

        /// <summary>
        /// The list of additional format-specific attributes on this symbol
        /// </summary>
        public IDictionary<string, IConvertible> Attributes { get; } = new Dictionary<string, IConvertible>();

        /// <param name="other">The other symbol</param>
        /// <returns>
        /// Whether or not all attributes of this symbols are equal to the attributes of the other symbol
        /// </returns>
        public bool AreAttributesEqual(Symbol other) =>
            Attributes.Keys.Count == other.Attributes.Keys.Count &&
            Attributes.Keys.All(k => other.Attributes.ContainsKey(k) && Attributes[k].Equals(other.Attributes[k]));

        #endregion

        #region Symbol Definition Methods

        public void DefineAsAbsolute(ulong value)
        {
            if (Target != SymbolTarget.UNDEFINED)
                throw new InvalidSymbolException("Tried redefining symbol target type", this);

            Target = SymbolTarget.ABSOLUTE;
            AbsoluteValue = value;
        }

        public void DefineAsSection(Section section)
        {
            if (Target != SymbolTarget.UNDEFINED)
                throw new InvalidSymbolException("Tried redefining symbol target type", this);

            Target = SymbolTarget.SECTION;
            TargetSection = section.GetHandle();
        }

        public void DefineAsLabel(Chunk chunk, ulong memCellOffset)
        {
            if (Target != SymbolTarget.UNDEFINED)
                throw new InvalidSymbolException("Tried redefining symbol target type", this);

            Target = SymbolTarget.LABEL;
            TargetChunk = chunk.GetHandle();
            MemCellOffset = memCellOffset;
        }

        #endregion

        #region Merging and Uniqueness

        /// <summary>
        /// Given two symbols, determines how they should merge (or not)
        /// </summary>
        /// <remarks>
        /// This method only verifies merge compatibility between the binding types, and optionally attributes.
        /// </remarks>
        /// <param name="existingSymbol">The existing symbol</param>
        public SymbolMergeBehaviour DetermineMergeBehaviour(Symbol existingSymbol)
        {
            bool incomingIsDefined = Target != SymbolTarget.UNDEFINED;
            bool existingIsDefined = existingSymbol.Target != SymbolTarget.UNDEFINED;
            bool incomingIsLocal = BindingType == SymbolBindType.LOCAL;
            bool existingIsLocal = existingSymbol.BindingType == SymbolBindType.LOCAL;

            // Rule 1: Undefined locals cannot participate in merging
            if ((!incomingIsDefined && incomingIsLocal) || (!existingIsDefined && existingIsLocal))
                return SymbolMergeBehaviour.INVALID_MERGE;

            // Rule 2: if either symbol is local, keep both
            if (incomingIsLocal || existingIsLocal)
                return SymbolMergeBehaviour.KEEP_BOTH;

            // Rule 3: if either symbol has a plugin binding type, defer to the output formatter
            if (BindingType == SymbolBindType.PLUGIN || existingSymbol.BindingType == SymbolBindType.PLUGIN)
                return SymbolMergeBehaviour.PLUGIN_DEPENDENT;

            // Now we only have weak and strong global symbols

            // Rule 4: Externs always lose to definitions
            switch (!incomingIsDefined, !existingIsDefined)
            {
                case (false, true): // if the existing symbol is an extern, the incoming symbol becomes the definition
                    return SymbolMergeBehaviour.USE_INCOMING;
                case (true, false): // if the incoming symbol is an extern, the existing symbol becomes the definition
                    return SymbolMergeBehaviour.USE_EXISTING;
                case (true, true): // if neither or both are externs, choose based on strength instead
                case (false, false):
                    break;
            }

            // Rule 5: In the event that both are defined or undefined, we choose based on strength
            switch (BindingType, existingSymbol.BindingType)
            {
                case (SymbolBindType.WEAK, SymbolBindType.WEAK): // If the incoming symbol is weak, it cannot pre-empt the existing symbol
                case (SymbolBindType.WEAK, SymbolBindType.GLOBAL):
                    return SymbolMergeBehaviour.USE_EXISTING;
                case (SymbolBindType.GLOBAL, SymbolBindType.WEAK): // Global symbols may pre-empt weak symbols
                    return SymbolMergeBehaviour.USE_INCOMING;
                case (SymbolBindType.GLOBAL, SymbolBindType.GLOBAL): // If both are global, choose based on whether it's defined or not
                    break;
            }

            // Rule 6: When there are two strong globals, they can only merge if their attributes are equal
            if (existingIsDefined && !AreAttributesEqual(existingSymbol))
                return SymbolMergeBehaviour.INVALID_MERGE;

            // Rule 7: if the merge is valid, we keep the existing symbol. The attributes of an incoming extern are lost if they differ
            return SymbolMergeBehaviour.USE_EXISTING;
        }

        public bool CanMergeInto(IMergeable destination)
        {
            GuardValidity();

            if (destination is not Symbol)
                return false;

            Symbol otherSymbol = (Symbol)destination;
            otherSymbol.GuardValidity();

            if (!EntityEquivalent(otherSymbol))
                return false;

            return !ReferenceEquals(this, destination);
        }

        public void MergeInto(IMergeable destination)
        {
            if (!CanMergeInto(destination))
                throw new CannotMergeException("Could not merge two symbols.", this, destination);

            Symbol destSymbol = (Symbol)destination;

            // this can only be USE_EXISTING, USE_INCOMING, or PLUGIN_DEPENDENT
            switch (DetermineMergeBehaviour(destSymbol))
            {
                case SymbolMergeBehaviour.USE_EXISTING:
                    break;
                case SymbolMergeBehaviour.USE_INCOMING:
                    destSymbol.Target = Target;
                    destSymbol.BindingType = BindingType;
                    destSymbol.AbsoluteValue = AbsoluteValue;
                    destSymbol.TargetSection = TargetSection?.Ref!.GetHandle();
                    destSymbol.TargetChunk = TargetChunk?.Ref!.GetHandle();
                    destSymbol.MemCellOffset = MemCellOffset;
                    break;
                case SymbolMergeBehaviour.PLUGIN_DEPENDENT:
                    destSymbol.OwningProgram.SharedMeta.Outputter.MergeSymbols(this, destSymbol); // CanMergeInto already checked validity
                    return;
                default:
                    throw new InvalidSymbolException("Invalid symbol state during merge", this);
            }

            // Cleanup the handles if they exist
            OnInvalidated();
        }

        public int EntityHash() => FullyQualifiedName.GetHashCode();

        public bool EntityEquivalent(ModelEntity other)
        {
            if (other is not Symbol)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            Symbol otherSymbol = (Symbol)other;

            if (FullyQualifiedName != otherSymbol.FullyQualifiedName)
                return false;

            switch (DetermineMergeBehaviour(otherSymbol))
            {
                case SymbolMergeBehaviour.INVALID_MERGE:
                case SymbolMergeBehaviour.KEEP_BOTH:
                    return false;

                case SymbolMergeBehaviour.USE_EXISTING:
                case SymbolMergeBehaviour.USE_INCOMING:
                    return true;

                case SymbolMergeBehaviour.PLUGIN_DEPENDENT:
                    return OwningProgram.SharedMeta.Outputter.AreSymbolsEquivalent(this, otherSymbol);
            }

            throw new InvalidSymbolException("Invalid merge behaviour from symbol", this);
        }

        #endregion

        protected override void OnInvalidated()
        {
            TargetSection = null;
            TargetChunk = null;
        }
    }
}
