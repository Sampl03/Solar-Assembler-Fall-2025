using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Meta
{
    public abstract class MetaEntity : ModelEntity
    {
        /// <summary>
        /// The Program in which this meta entity was declared
        /// </summary>
        public Program OwningProgram
        {
            get
            {
                GuardValidity();
                return (Program)OwningTable!.Context;
            }
        }
    }
}
