using Solar.EntitySystem;
using Solar.EntitySystem.Behavior;
using Solar.EntitySystem.Exceptions;

using Solar.Asm.Engine.Model.Code;
using Solar.Asm.Engine.Model.Meta;
using Solar.Asm.Engine.Model.Symbols;
using Solar.Asm.Engine.Model.Expressions;
using Solar.Asm.Engine.Model.IO;

namespace Solar.Asm.Engine.Model
{
    /// <summary>
    /// Top-level Context for Solar ML Assembler programs.<br/>
    /// Contains the <see cref="EntityManager"/> instances to which all Model entities belong.
    /// </summary>
    /// <remarks>
    /// The specific <see cref="Program"/> instance should be provided by an <see cref="OutputFormatter">
    /// </remarks>
    public abstract class Program : IContext, IMergeable
    {
        public ArchitectureSpecs ArchSpecs { get => SharedMeta.AssemblyDialect.ArchSpecs; }

        public required SharedMetaContext SharedMeta { get; init; }
        public required InputReader Inputter { get; init; }

        public EntityManager CodeEntities { get; init; }
        public EntityManager Symbols { get; init; }
        public EntityManager Expressions { get; init; }

        public Program(in SharedMetaContext sharedMeta, InputReader inputter)
        {
            SharedMeta      = sharedMeta;
            Inputter        = inputter;
            CodeEntities    = new(this, typeof(CodeEntity)   );
            Symbols         = new(this, typeof(Symbol)       );
            Expressions     = new(this, typeof(ExpressionBase) );
        }

        public abstract Section CreateOrGetSection();

        public abstract Symbol CreateOrGetSymbol();

        public abstract void FinalizeSymbols();

        public virtual bool CanMergeInto(IMergeable destination)
        {
            // The two programs must be of the same type (they should come from the same OutputFormatter)
            if (GetType() != destination.GetType())
                return false;

            // Recast for ease of use
            Program destProgram = (Program)destination;

            // Architectures must be compatible
            if (ArchSpecs != destProgram.ArchSpecs)
                return false;

            // Managers must be able to merge
            if (!CodeEntities.CanMergeInto(destProgram.CodeEntities))
                return false;
            if (!Symbols.CanMergeInto(destProgram.Symbols))
                return false;
            if (!Expressions.CanMergeInto(destProgram.Expressions))
                return false;

            return true;
        }

        public virtual void MergeInto(IMergeable destination)
        {
            if (!CanMergeInto(destination))
                throw new CannotMergeException($"Could not merge Program into an entity of type {destination.GetType()}", this, destination);

            // Recast for ease of use
            Program destProgram = (Program)destination;

            // Merge destination program's managers into ours
            CodeEntities.MergeInto(destProgram.CodeEntities);
            Symbols.MergeInto(destProgram.Symbols);
            Expressions.MergeInto(destProgram.Expressions);

        }
    }
}
