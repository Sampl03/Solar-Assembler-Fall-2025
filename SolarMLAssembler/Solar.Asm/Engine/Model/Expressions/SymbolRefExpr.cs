using Solar.Asm.Engine.Model.Symbols;
using Solar.EntitySystem;

namespace Solar.Asm.Engine.Model.Expressions
{
    public sealed class SymbolRefExpr : Expression<ulong>
    {
        public override bool IsConstantExpression => Symbol.Target == SymbolTarget.ABSOLUTE;

        private readonly EntityHandle<Symbol> _symbol;
        public Symbol Symbol => _symbol.Ref!;

        // Private constructor to enforce the use of the From factory method
        private SymbolRefExpr(Symbol symbol)
        {
            _symbol = symbol.GetHandle();
        }

        /// <summary>
        /// Creates a new SymbolRefExpr from a Symbol.
        /// </summary>
        /// <param name="context">The <see cref="Program"/> this literal expression should exist within</param>
        /// <param name="symbol">The <see cref="Symbols.Symbol"/> to reference</param>
        public static SymbolRefExpr From(Program context, Symbol symbol)
        {
            symbol.GuardValidity();
            SymbolRefExpr newExpr = new(symbol);
            newExpr.Initialise(context.Expressions);
            return newExpr;
        }

        public override ExpressionResult<ulong> Evaluate()
        {
            GuardValidity();

            return new()
            {
                HasValue = true,
                Value = Symbol.GetValue()
            };
        }

        public override void Simplify()
        {
            GuardValidity();

            // If the symbol isn't absolute, we can't simplify
            if (!IsConstantExpression)
                return;

            // Replace this expression with a literal expression of the result value
            this.ReplaceWith(
                LiteralExpr<ulong>.FromValue(
                    OwningProgram,
                    Symbol.GetValue()
                )
            );
        }

        protected override void OnInvalidated()
        {
            _symbol.Dispose();
        }
    }
}
