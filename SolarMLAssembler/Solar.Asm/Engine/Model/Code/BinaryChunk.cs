
namespace Solar.Asm.Engine.Model.Code
{
    public sealed class BinaryChunk(byte[] data, BinaryPatch[] patches) : Chunk
    {
        public override IReadOnlyList<byte> EmitBytes()
        {
            GuardValidity();

            if (patches.Length > 0)
                data = OwningProgram.SharedMeta.AssemblyDialect.PatchBytes(data, patches);

            return (byte[])data.Clone();
        }

        public override BinaryPatch[] EmitPatches() {
            GuardValidity();

            return (BinaryPatch[])patches.Clone();
        }
    }
}
