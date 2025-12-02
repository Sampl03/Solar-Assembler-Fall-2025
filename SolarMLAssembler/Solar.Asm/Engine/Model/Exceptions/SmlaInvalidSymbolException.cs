using Solar.Asm.Engine.Model.Symbols;

namespace Solar.Asm.Engine.Model.Exceptions
{
    public class SmlaInvalidSymbolException : SmlaException
    {
        public readonly Symbol? Symbol = null;

        public SmlaInvalidSymbolException()
        {
        }

        public SmlaInvalidSymbolException(string message) : base(message)
        {
        }

        public SmlaInvalidSymbolException(string message, Exception inner) : base(message, inner)
        {
        }

        public SmlaInvalidSymbolException(string message, Symbol symbol) : this(message)
        {
            Symbol = symbol;
        }

        public SmlaInvalidSymbolException(string message, Symbol symbol, Exception inner) : this(message, inner)
        {
            Symbol = symbol;
        }
    }
}
