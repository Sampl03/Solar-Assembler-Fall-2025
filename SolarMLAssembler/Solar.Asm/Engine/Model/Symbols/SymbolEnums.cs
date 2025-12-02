namespace Solar.Asm.Engine.Model.Symbols
{
    public enum SymbolTarget
    {
        /// <summary>Symbol is not yet assigned a target type (extern)</summary>
        UNDEFINED = 0,
        /// <summary>Symbol is a constant, absolute value</summary>
        ABSOLUTE = 1,
        /// <summary>Symbol is attached to a section</summary>
        SECTION = 2,
        /// <summary>Symbol is attached to a chunk</summary>
        LABEL = 3
    }

    public enum SymbolBindType
    {
        /// <summary>Symbol binding type is set in a plugin attribute</summary>
        PLUGIN = 0,
        /// <summary>Symbol is local to its source file</summary>
        LOCAL = 1,
        /// <summary>Symbol is visible across multiple sources and can pre-empt weak symbols</summary>
        GLOBAL = 2,
        /// <summary>Symbol is visible across multiple sources but cannot pre-empt other symbols</summary>
        WEAK = 3
    }

    public enum SymbolMergeBehaviour
    {
        /// <summary>The two symbols cannot merge and cannot coexist</summary>
        INVALID_MERGE = 0,
        /// <summary>The two symbols can coexist and should not merge</summary>
        KEEP_BOTH = 1,
        /// <summary>Keep the existing symbol's values and transfer the incoming symbol's handles to the new one</summary>
        USE_EXISTING = 2,
        /// <summary>Overwrite the existing symbol's values with the incoming symbol's, and then transfer the handles, effectively overriding the existing definition</summary>
        USE_INCOMING = 3,
        /// <summary>Merging behaviour is implemented by OutputFormatter</summary>
        PLUGIN_DEPENDENT = 4
    }
}