namespace OGS.HOCON.Impl
{
    /// <summary>
    /// The token handle.
    /// </summary>
    internal class TokenHandle
    {
        /// <summary>
        /// The _next offset.
        /// </summary>
        private readonly int _nextOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenHandle"/> class.
        /// </summary>
        public TokenHandle(string value, int nextOffset)
        {
            Value = value;
            _nextOffset = nextOffset;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// The consume.
        /// </summary>
        public int Consume()
        {
            return _nextOffset;
        }
    }
}