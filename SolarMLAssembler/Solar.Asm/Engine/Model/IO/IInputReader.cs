using Solar.Asm.Engine.Model.Symbols;
using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.IO
{
    public interface IInputReader
    {
        public bool IsCompatibleWithArchitecture(SharedMetaContext sharedMetaContext);

        public static abstract IInputReader Create(SharedMetaContext sharedMetaContext, Program program);

        public Program Program { get; }

        public void ReadProgram(Stream input);
    }
}
