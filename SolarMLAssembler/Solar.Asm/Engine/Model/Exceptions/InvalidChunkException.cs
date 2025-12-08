
using Solar.Asm.Engine.Model.Code;

namespace Solar.Asm.Engine.Model.Exceptions
{
    public class InvalidChunkException : SmlaException
    {
        public Chunk? Chunk;

        public InvalidChunkException()
        {
        }

        public InvalidChunkException(string message) : base(message)
        {
        }

        public InvalidChunkException(string message, Exception inner) : base(message, inner)
        {
        }

        public InvalidChunkException(string message, Chunk chunk) : this(message)
        {
            Chunk = chunk;
        }

        public InvalidChunkException(string message, Chunk chunk, Exception inner) : this(message, inner)
        {
            Chunk = chunk;
        }
    }
}
