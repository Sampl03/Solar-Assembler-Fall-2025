using Solar.Asm.Engine.Model;
using Solar.Asm.Engine.Model.IO;

namespace Demo.Mos6502
{
    public sealed class Mos6502Reader : IInputReader
    {
        public Program Program => throw new NotImplementedException();

        public static IInputReader Create(SharedMetaContext sharedMetaContext, Program program)
        {
            throw new NotImplementedException();
        }

        public bool IsCompatibleWithArchitecture(SharedMetaContext sharedMetaContext)
        {
            throw new NotImplementedException();
        }

        public void ReadProgram(Stream input)
        {
            throw new NotImplementedException();
        }
    }
}
