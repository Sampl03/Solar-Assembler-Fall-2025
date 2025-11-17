namespace Solar.Asm.Engine.Model.Code
{
    public abstract class Chunk : CodeEntity
    {
        protected Chunk() : base()
        {
        }

        public abstract byte[] ToBytes();

        public abstract int SizeBytes();

        public abstract bool Shrink();
    }
}
