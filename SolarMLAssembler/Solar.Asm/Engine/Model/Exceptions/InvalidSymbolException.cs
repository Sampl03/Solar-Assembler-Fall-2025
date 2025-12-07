using Solar.Asm.Engine.Model.Symbols;

namespace Solar.Asm.Engine.Model.Exceptions
{
    public class InvalidSymbolException : SmlaException
    {
        public readonly Symbol? Symbol = null;

        public InvalidSymbolException()
        {
        }

        public InvalidSymbolException(string message) : base(message)
        {
        }

        public InvalidSymbolException(string message, Exception inner) : base(message, inner)
        {
        }

        public InvalidSymbolException(string message, Symbol symbol) : this(message)
        {
            Symbol = symbol;
        }

        public InvalidSymbolException(string message, Symbol symbol, Exception inner) : this(message, inner)
        {
            Symbol = symbol;
        }
    }
}
