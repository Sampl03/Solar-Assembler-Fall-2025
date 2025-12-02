using Solar.Asm.Engine.Model.Symbols;
using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Meta.IO
{
    public abstract class InputReader
    {
        public abstract bool IsCompatibleWithArchitecture(ArchitectureSpecs architectureSpecs);

        public abstract bool TryReadProgram(out bool Program);

        public abstract EntityHandle<Symbol>? TryFindSymbol(QualifiedName name);

        public abstract EntityHandle<Symbol> FindOrCreateSymbol(QualifiedName name);
    }
}
