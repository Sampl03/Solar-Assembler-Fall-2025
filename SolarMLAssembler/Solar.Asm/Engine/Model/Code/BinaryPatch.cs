using Solar.Asm.Engine.Model.IO;
using Solar.Asm.Engine.Model.Expressions;
using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Code
{
    /// <summary>
    /// Represents a patch/relocation to be applied to a code entity's memory cell representation
    /// </summary>
    public struct BinaryPatch
    {

        /// <summary>
        /// A string ID representative of the patch type. This should be understood by the <see cref="AssemblyParser"/>
        /// </summary>
        public readonly string PatchTypeID;

        /// <summary>Offset of the patch within the memory cell representation</summary>
        public ulong  CellOffset;

        // All emitted patches use the same handle as the emitter chunk.
        private readonly EntityHandle<Expression<ulong>> _valueExprHandle;
        /// <summary>
        /// The expression which computes the patch value to use
        /// </summary>
        public readonly Expression<ulong> ValueExpr => _valueExprHandle.Ref!;

        public BinaryPatch(string patchTypeID, ulong cellOffset, EntityHandle<Expression<ulong>> valueExpression)
        {
            PatchTypeID = patchTypeID;
            CellOffset = cellOffset;
            _valueExprHandle = valueExpression;
        }
    }
}
