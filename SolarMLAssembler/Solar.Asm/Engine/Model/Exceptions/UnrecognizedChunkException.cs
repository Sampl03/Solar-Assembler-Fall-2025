
namespace Solar.Asm.Engine.Model.Exceptions
{
    public class UnrecognizedChunkException : SmlaException
    {
        public string? Opcode;

        public UnrecognizedChunkException()
        {
        }

        public UnrecognizedChunkException(string message) : base(message)
        {
        }

        public UnrecognizedChunkException(string message, Exception inner) : base(message, inner)
        {
        }

        public UnrecognizedChunkException(string message, string opcode) : this(message)
        {
            Opcode = opcode;
        }

        public UnrecognizedChunkException(string message, string opcode, Exception inner) : this(message, inner)
        {
            Opcode = opcode;
        }
    }
}
