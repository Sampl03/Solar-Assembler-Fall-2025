using Solar.Asm.Engine.Model.Code;
using Solar.Asm.Engine.Model.Symbols;

namespace Solar.Asm.Engine.Model.Meta.IO
{
    public abstract class OutputFormatter
    {
        public abstract bool IsCompatibleWithArchitecture(ArchitectureSpecs architectureSpecs);

        public abstract Section GetNewSection();

        protected internal abstract void CanMergeSymbols(Symbol incoming, Symbol destination);

        protected internal abstract void MergeSymbols(Symbol incoming, Symbol destination);

        protected internal abstract bool AreSymbolsEquivalent(Symbol symbol1, Symbol symbol2);
    }
}
