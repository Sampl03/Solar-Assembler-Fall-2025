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

        public bool CanMerge(IMergeable other)
        {
            // Can't merge a non-Program into self
            if (typeof(Program) != other.GetType())
                return false;

            // Recast for ease of use
            Program otherProgram = (Program)other;

            // Managers must be able to merge
            if (!CodeEntities.CanMerge(otherProgram.CodeEntities))
                return false;
            if (!Meta.CanMerge(otherProgram.Meta))
                return false;
            if (!Symbols.CanMerge(otherProgram.Symbols))
                return false;
            if (!Expressions.CanMerge(otherProgram.Expressions))
                return false;

            return true;
        }

        public void Merge(IMergeable other)
        {
            if (!CanMerge(other))
                throw new CannotMergeException("Cannot merge Program with an entity that is not also a Program", this, other);

            // Recast for ease of use
            Program otherProgram = (Program)other;

            // Merge
            CodeEntities.Merge(otherProgram.CodeEntities);
            Meta.Merge(otherProgram.Meta);
            Symbols.Merge(otherProgram.Symbols);
            Expressions.Merge(otherProgram.Expressions);

        }
    }
}
