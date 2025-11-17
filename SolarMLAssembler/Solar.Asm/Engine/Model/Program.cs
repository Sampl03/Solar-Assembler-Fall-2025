using Solar.EntitySystem;
using Solar.EntitySystem.Behavior;
using Solar.EntitySystem.Exceptions;

using Solar.Asm.Engine.Model.Code;
using Solar.Asm.Engine.Model.Meta;
using Solar.Asm.Engine.Model.Symbols;
using Solar.Asm.Engine.Model.Expressions;

namespace Solar.Asm.Engine.Model
{
    /// <summary>
    /// Top-level Context for Solar ML Assembler programs.<br/>
    /// Contains the <see cref="EntityManager"/> instances to which all Model entities belong.
    /// </summary>
    public class Program : IContext, IMergeable
    {
        public EntityManager CodeEntities { get; init; }
        public EntityManager Meta { get; init; }
        public EntityManager Symbols { get; init; }
        public EntityManager Expressions { get; init; }

        public Program()
        {
            CodeEntities    = new(this, typeof(CodeEntity)   );
            Meta            = new(this, typeof(MetaEntity)   );
            Symbols         = new(this, typeof(Symbol)       );
            Expressions     = new(this, typeof(Expression<>) );
        }

        public bool CanMergeInto(IMergeable destination)
        {
            // Can't merge into a non-program
            if (destination is not Program)
                return false;

            // Recast for ease of use
            Program destProgram = (Program)destination;

            // Managers must be able to merge
            if (!CodeEntities.CanMergeInto(destProgram.CodeEntities))
                return false;
            if (!Meta.CanMergeInto(destProgram.Meta))
                return false;
            if (!Symbols.CanMergeInto(destProgram.Symbols))
                return false;
            if (!Expressions.CanMergeInto(destProgram.Expressions))
                return false;

            return true;
        }

        public void MergeInto(IMergeable destination)
        {
            if (!CanMergeInto(destination))
                throw new CannotMergeException($"Could not merge Program into an entity of type {destination.GetType()}", this, destination);

            // Recast for ease of use
            Program destProgram = (Program)destination;

            // Merge destination program's managers into ours
            CodeEntities.MergeInto(destProgram.CodeEntities);
            Meta.MergeInto(destProgram.Meta);
            Symbols.MergeInto(destProgram.Symbols);
            Expressions.MergeInto(destProgram.Expressions);

        }
    }
}
