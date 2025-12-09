using Solar.Asm.Engine.Model.IO;
using Solar.Asm.Engine.Model.Meta;
using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model
{
    /// <summary>
    /// Top-level context for shared meta-enties across multiple programs in a single session
    /// </summary>
    public readonly record struct SharedMetaContext : IContext
    {
        public readonly IOutputFormatter Outputter;
        public readonly IAssemblyParser AssemblyDialect;
        private readonly EntityManager Meta;

        public SharedMetaContext(IOutputFormatter outputter, IAssemblyParser assemblyDialect)
        {
            Outputter = outputter;
            AssemblyDialect = assemblyDialect;
            Meta = new(this, typeof(MetaEntity));
        }
    }
}
