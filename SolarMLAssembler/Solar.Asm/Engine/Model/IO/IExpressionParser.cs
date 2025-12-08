using Solar.Asm.Engine.Model.Expressions;

namespace Solar.Asm.Engine.Model.IO
{
    public interface IExpressionParser
    {
        /// <summary>
        /// Parse an expression of an arbitrary type
        /// </summary>
        /// <remarks>
        /// The caller is responsible for verifying the type of expression returned
        /// </remarks>
        /// <param name="context">The <see cref="Program"/> context to add the expression to</param>
        /// <param name="source">The reader positioned at the start of the string to parse</param>
        public ExpressionBase ParseExpression(Program context, in TrackingReader source);
    }
}
