using Solar.Asm.Engine.Model.Symbols;

namespace Solar.Asm.Engine.Model.Exceptions
{
    public class UnresolvedSymbolException : InvalidSymbolException
    {
        public UnresolvedSymbolException()
        {
        }

        public UnresolvedSymbolException(string message) : base(message)
        {
        }

        public UnresolvedSymbolException(string message, Exception inner) : base(message, inner)
        {
        }

        public UnresolvedSymbolException(string message, Symbol symbol) : base(message, symbol)
        {
        }

        public UnresolvedSymbolException(string message, Symbol symbol, Exception inner) : base(message, symbol, inner)
        {
        }
    }
}
