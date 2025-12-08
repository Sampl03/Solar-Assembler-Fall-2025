using Solar.Asm.Engine.Model;
using Solar.Asm.Engine.Model.IO;
using Solar.Asm.Engine.Model.Symbols;

namespace Demo.Mos6502
{
    public sealed class Easy6502Formatter : IOutputFormatter
    {
        public static SharedMetaContext Create(IAssemblyParser assemblyParser)
        {
            throw new NotImplementedException();
        }

        public static bool IsCompatibleWithArchitecture(ArchitectureSpecs architectureSpecs)
        {
            throw new NotImplementedException();
        }

        public Program CreateProgram()
        {
            throw new NotImplementedException();
        }

        public Stream EmitProgram(Program program, Stream output)
        {
            throw new NotImplementedException();
        }

        bool IOutputFormatter.AreSymbolsEquivalent(Symbol symbol1, Symbol symbol2)
        {
            throw new NotImplementedException();
        }

        void IOutputFormatter.MergeSymbols(Symbol incoming, Symbol destination)
        {
            throw new NotImplementedException();
        }
    }
}
