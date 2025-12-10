
namespace Solar.Asm.Engine.Model.Code
{
    public sealed class BinaryChunk : Chunk
    {
        private byte[] _data;
        private BinaryPatch[] _patches;

        public BinaryChunk(byte[] data, BinaryPatch[] patches)
        {
            _data = (byte[])data.Clone();
            _patches = (BinaryPatch[])patches.Clone();
        }

        public override IReadOnlyList<byte> EmitBytes()
        {
            GuardValidity();

            if (_patches.Length > 0)
                _data = OwningProgram.SharedMeta.AssemblyDialect.PatchBytes(CalculateMemCellVirtualAddress(), _data, _patches);

            return (byte[])_data.Clone();
        }

        public override BinaryPatch[] EmitPatches() {
            GuardValidity();

            return (BinaryPatch[])_patches.Clone();
        }
    }
}
