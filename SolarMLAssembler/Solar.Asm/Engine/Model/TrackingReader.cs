namespace Solar.Asm.Engine.Model
{
    /// <summary>
    /// Wraps a <see cref="TextReader"/>, keeping track of the current line
    /// and the current column.<br/>
    /// <br/>
    /// Normalizes line endings to <c>\n</c>,
    /// and automatically merges lines ending in <c>\\\n</c> or <c>\\\r\n</c>
    /// <br/>
    /// Exposes basic tokenizing functions as well
    /// </summary>
    /// <param name="inner">
    /// The _inner text reader to wrap
    /// </param>
    public sealed class TrackingReader : TextReader
    {
        /// <summary>
        /// Struct used to indicate a logical character peek, which may read multiple characters
        /// </summary>
        private readonly struct LogicalChar
        {
            public readonly int Value { get; init; }
            public readonly int CharsRead { get; init; }
            public readonly bool PhysicalNewline { get; init; }

            public LogicalChar(int value, int charsRead, bool physicalNewline)
            {
                Value = value;
                CharsRead = charsRead;
                PhysicalNewline = physicalNewline;
            }
        }

        /// <summary>
        /// The number of characters read from the _inner reader
        /// </summary>
        /// <remarks>
        /// This may not be equal to the number of characters read from this reader because of normalization
        /// </remarks>
        public long CharsRead { get; private set; } = 0;

        /// <summary>
        /// Returns the current line number after the previous read
        /// </summary>
        /// <remarks>
        /// Note that this does not get affected by the backslash newline escape sequence
        /// </remarks>
        public long CurrentLine { get; private set; } = 1;

        /// <summary>
        /// Returns the current column index
        /// </summary>
        public long CurrentColumn { get; private set; } = 0;

        private Queue<int> _buffer = [];

        private TextReader _inner;
        public TrackingReader(TextReader inner)
        {
            _inner = inner;

            // Buffer must be at least 3 values ahead
            for (int i = 0; i < 3; i++)
                _buffer.Enqueue(inner.Read());
        }

        private int InnerRead()
        {
            int c = _buffer.Dequeue();
            _buffer.Enqueue(_inner.Read());

            CharsRead++;
            return c;
        }

        private LogicalChar PeekLogicalChar()
        {
            int c1 = _buffer.First();
            int c2 = _buffer.ElementAt(1);
            int c3 = _buffer.ElementAt(2);

            // If EOF, return EOF
            if (c1 == -1)
                return new(-1, 0, false);

            // Handle newline escape ("\" at the end of a line)
            if (c1 == '\\')
            {
                // if it's not followed by a new line, return it normally
                if (!(c2 == '\r' || c2 == '\n'))
                    return new(c1, 1, false);

                // Check which kind of newline
                switch (c2, c3)
                {
                    case ('\n', _): return new(' ', 2, true); // Linux
                    case ('\r', '\n'): return new(' ', 3, true); // Windows
                    case ('\r', _): return new(' ', 2, true); // Old Macs
                }
            }

            // Handle newline normalization
            if (c1 == '\r' || c1 == '\n')
            {
                // Check which kind of newline
                switch (c1, c2)
                {
                    case ('\n', _): return new('\n', 1, true); // Linux
                    case ('\r', '\n'): return new('\n', 2, true); // Windows
                    case ('\r', _): return new('\n', 1, true); // Old Macs
                }
            }

            // Otherwise just return the character
            return new(c1, 1, false);
        }

        public override int Read()
        {
            LogicalChar c = PeekLogicalChar();

            // If EOF, do nothing
            if (c.Value == -1)
                return -1;

            // Otherwise read and update values
            for (int i = 0; i < c.CharsRead; i++)
                InnerRead();

            if (c.PhysicalNewline)
            {
                CurrentLine += 1;
                CurrentColumn = 0;
            }

            return c.Value;
        }

        public override int Peek() => PeekLogicalChar().Value;

        public void SkipWhitespace(bool skipNewLines = false)
        {
            while (true)
            {
                var peek = Peek();
                if (Peek() == -1)
                    break;

                if (!char.IsWhiteSpace((char)peek))
                    break;

                if (!skipNewLines && peek == '\n')
                    break;

                Read();
            }
        }
    }
}
