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
    /// The inner text reader to wrap
    /// </param>
    public sealed class TrackingReader(TextReader inner) : TextReader
    {
        /// <summary>
        /// The number of characters read from the inner reader
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

        private int InnerRead()
        {
            int c = inner.Read();
            if (c != -1) CharsRead++;
            return c;
        }

        public override int Read()
        {
            int c = InnerRead();

            // If EOF, do nothing
            if (c == -1)
                return -1;

            // Handle newline escape ("\" at the end of a line)
            if (c == '\\')
            {
                // if it's not followed by a newline, return it normally
                if (!(Peek() == '\r' || Peek() == '\n'))
                    return c;

                // otherwise we treat the newline by calling recursively
                Read();
                return ' ';
            }

            // Handle newline normalization
            if (c == '\r' || c == '\n')
            {
                int temp = InnerRead();
                if (temp == '\r' && Peek() == '\n') // newline normalization
                    InnerRead();
                return '\n'; // Gets treated as a single \n
            }

            // Otherwise return the real character
            return c;
        }

        public override int Peek() => inner.Peek();

        public void SkipWhitespace()
        {
            while (Peek() != -1 && char.IsWhiteSpace((char)Peek()))
                Read();
        }
    }
}
