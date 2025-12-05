using Solar.Asm.Engine.Model.Meta.IO;
using Solar.Asm.Engine.Model.Meta;
using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model
{
    /// <summary>
    /// Top-level context for shared meta-enties across multiple programs in a single session
    /// </summary>
    public sealed class SharedMetaContext : IContext
    {
        public readonly OutputFormatter Outputter;
        public readonly AssemblyParser AssemblyDialect;
        private readonly EntityManager Meta;

        public SharedMetaContext(OutputFormatter outputter, AssemblyParser assemblyDialect)
        {
            Outputter = outputter;
            AssemblyDialect = assemblyDialect;
            Meta = new(this, typeof(MetaEntity));
        }
    }
}
