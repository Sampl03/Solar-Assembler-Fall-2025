using Solar.Asm.Engine.Model.Code;

namespace Solar.Asm.Engine.Model.IO
{
    /// <summary>
    /// Handles the translation and patching of assembly source and assembled code specific to an architecture
    /// </summary>
    public abstract class AssemblyParser
    {
        public ArchitectureSpecs ArchSpecs { get; init; }

        public abstract Chunk ParseInstruction(Program context, string source);

        public abstract byte[] PatchBytes(byte[] data, ChunkPatchRecord[] patches);
    }
}
