using Solar.Asm.Engine.Model.Exceptions;
using Solar.Asm.Engine.Model.Symbols;

namespace Solar.Asm.Engine.Model.IO
{
    public interface IOutputFormatter
    {
        public static abstract bool IsCompatibleWithArchitecture(ArchitectureSpecs architectureSpecs);

        /// <summary>
        /// Verifies that this formatter is compatible with the specified assembly parser,
        /// and returns a <see cref="SharedMetaContext"/> containing references to both
        /// </summary>
        /// <remarks>
        /// Throws <see cref="IncompatibleArchitectureException"/> if the assembly parser is incompatible
        /// </remarks>
        /// <param name="assemblyParser"><</param>
        /// <exception cref="IncompatibleArchitectureException"/>
        public static abstract SharedMetaContext Create(IAssemblyParser assemblyParser);

        /// <returns>
        /// A new and empty <see cref="Program"/> data model
        /// </returns>
        public Program CreateProgram();

        /// <summary>
        /// Outputs the specified <see cref="Program"/> model to a byte stream
        /// </summary>
        /// <remarks>
        /// Throws <see cref="CannotOutputException"/> if the <see cref="Program"/> data model cannot be
        /// serialized by this formatter
        /// </remarks>
        /// <param name="program"></param>
        /// <exception cref="CannotOutputException"/>
        public Stream EmitProgram(Program program, Stream output);

        /// <summary>
        /// Merges two symbols according to plugin-dependent logic.
        /// </summary>
        /// <remarks>
        /// This is called by <see cref="Symbol"/> instances during merging if either of them has
        /// a binding type set <see cref="SymbolBindType.PLUGIN"/> 
        /// </remarks>
        /// <param name="incoming"></param>
        /// <param name="destination"></param>
        protected internal abstract void MergeSymbols(Symbol incoming, Symbol destination);

        /// <summary>
        /// Compares whether two symbols are equivalent according to plugin-dependent logic.
        /// </summary>
        /// <remarks>
        /// This is called by <see cref="Symbol"/> instances during merging
        /// if either of them has a binding type set to <see cref="SymbolBindType.PLUGIN"/>
        /// </remarks>
        /// <param name="symbol1"></param>
        /// <param name="symbol2"></param>
        protected internal abstract bool AreSymbolsEquivalent(Symbol symbol1, Symbol symbol2);
    }
}
