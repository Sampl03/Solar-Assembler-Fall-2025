using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Code
{
    public class Fragment : CodeEntity
    {
        public IList<EntityHandle<Chunk>> Chunks { get; } = [];

        public Fragment() : base()
        {
        }
    }
}
