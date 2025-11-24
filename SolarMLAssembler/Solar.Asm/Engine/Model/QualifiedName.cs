namespace Solar.Asm.Engine.Model
{
    public class QualifiedNameException : Exception;

    /*
     * Represents an identifier with support for namespaces.
     * Different levels are separated by '::'
     * 
     * Conversion to and from string is supported
     */
    public record QualifiedName
    {
        private readonly string[] _names;

        #region Constants
        public const string NamespaceSeparator = "::";
        #endregion


        #region Properties
        public string Name => _names[^1];
        public QualifiedName Namespace => new QualifiedName(_names[..^1]);

        public int Depth => _names.Length;
        public bool IsSimple => _names.Length == 1;
        public bool IsEmpty => _names.Length == 0;
        #endregion


        #region Operators and Conversion
        // Convert string representation to QualifiedName
        public static implicit operator QualifiedName(string name) => new QualifiedName(name.Split(NamespaceSeparator));
        public static explicit operator string(QualifiedName qualifiedName) => qualifiedName.ToString();

        // Two qualified names are equivalent if they share the same full path
        public virtual bool Equals(QualifiedName? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (Depth != other.Depth)
                return false;

            for (int i = 0; i < Depth; i++)
                if (_names[i] != other._names[i])
                    return false;

            return true;
        }
        #endregion


        #region Constructors
        public QualifiedName(string[] arrayOfSimpleNames)
        {
            _names = arrayOfSimpleNames;
        }

        public QualifiedName(QualifiedName name, QualifiedName? @namespace = null)
        {
            if (@namespace == null)
            {
                _names = name._names;
            }
            else
            {
                _names = @namespace._names.Concat(name._names).ToArray();
            }
        }
        #endregion


        #region Methods
        public QualifiedName Concat(QualifiedName name) => new QualifiedName(name, this);

        // Returns the closest shared namespace between this and other
        public QualifiedName GetClosestSharedNamespace(QualifiedName other)
        {
            int shortestDepth = Math.Min(Depth, other.Depth);

            List<string> sharedNames = new List<string>();
            for (int i = 0; i < shortestDepth; i++)
            {
                if (_names[i] == other._names[i])
                {
                    sharedNames.Add(_names[i]);
                }
                else
                {
                    break;
                }
            }

            return new QualifiedName(sharedNames.ToArray());
        }

        // Relationship methods
        public bool IsAncestorOf(QualifiedName other) => this != other && GetClosestSharedNamespace(other) == this;
        public bool IsDescendantOf(QualifiedName other) => other.IsAncestorOf(this);
        public bool IsParentOf(QualifiedName other) => other.Namespace == this;
        public bool IsChildOf(QualifiedName other) => Namespace == other;
        public bool IsSiblingOf(QualifiedName other) => this != other && Namespace == other.Namespace;

        /* Shorthand for methods
         * 
         * Namespace + Name => Concat
         * Namespace & Namespace => GetClosestSharedNamespace
         * 
         * Ancestor >> Descendant
         * Descendant << Ancestor
         * Parent > Child
         * Child < Parent
         * Sibling ^ Sibling
         */
        public static QualifiedName operator +(QualifiedName @namespace, QualifiedName name) => @namespace.Concat(name);
        public static QualifiedName operator &(QualifiedName name1, QualifiedName name2) => name1.GetClosestSharedNamespace(name2);
        public static bool operator >>(QualifiedName ancestor, QualifiedName descendant) => ancestor.IsAncestorOf(descendant);
        public static bool operator <<(QualifiedName descendant, QualifiedName ancestor) => ancestor.IsAncestorOf(descendant);
        public static bool operator >(QualifiedName parent, QualifiedName descendant) => parent.IsParentOf(descendant);
        public static bool operator <(QualifiedName child, QualifiedName parent) => parent.IsParentOf(child);
        public static bool operator ^(QualifiedName sibling1, QualifiedName sibling2) => sibling1.IsSiblingOf(sibling2);

        // Overrides
        public override string ToString() => string.Join(NamespaceSeparator, _names);

        public override int GetHashCode() => ToString().GetHashCode();
        #endregion
    }
}
