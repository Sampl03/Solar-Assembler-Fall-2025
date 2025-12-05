using Solar.Asm.Engine.Model.IO;
using Solar.Asm.Engine.Model.Expressions;
using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Code
{
    /// <summary>
    /// Represents a patch/relocation to be applied to a chunk's binary representation
    /// </summary>
    public sealed class ChunkPatch : IDisposable
    {

        /// <summary>
        /// A string ID representative of the patch type. This should be understood by the <see cref="AssemblyParser"/>
        /// </summary>
        public readonly string PatchTypeID;

        /// <summary>Offset of the patch within the memory cell representation of the chunk</summary>
        public readonly ulong  CellOffset;

        /// <summary>
        /// A handle to the expression which computes the patch value to use
        /// </summary>
        public readonly EntityHandle<Expression<ulong>> ValueExpr;

        public ChunkPatch(string patchTypeID, ulong cellOffset, Expression<ulong> valueExpression)
        {
            PatchTypeID = patchTypeID;
            CellOffset = cellOffset;
            ValueExpr = valueExpression.GetHandle();
        }

        private bool disposedValue;
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                ValueExpr.Dispose();
                disposedValue = true;
            }
        }

        ~ChunkPatch() => Dispose(disposing: false);

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
