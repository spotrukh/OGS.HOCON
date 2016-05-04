namespace OGS.HOCON.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The token processor.
    /// </summary>
    internal class TokenProcessor
    {
        /// <summary> The tokens. </summary>
        private readonly List<Token> _tokens;

        /// <summary> The handle. </summary>
        private TokenHandle _handle;

        /// <summary> The content. </summary>
        private string _content;

        /// <summary> The offset backfield. </summary>
        private int _offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenProcessor"/> class.
        /// </summary>
        public TokenProcessor(string content, IEnumerable<Token> tokens)
        {
            _tokens = new List<Token>(tokens);
            _content = content;
        }

        /// <summary> Gets the offset. </summary>
        public int Offset
        {
            get { return _offset; }
        }

        /// <summary>
        /// The include.
        /// </summary>
        public void Include(string content)
        {
            _content = _content.Insert(_offset, Environment.NewLine + content + Environment.NewLine);
        }

        /// <summary>
        /// The read next.
        /// </summary>
        public bool ReadNext(out TokenType token, out string value, TokenType[] requestedTokens, TokenType[] ignoreTokens)
        {
            token = TokenType.Unknown;
            value = string.Empty;

            if (this.IsEof()) return false;

            bool tokenFound;

            // Read ignorable tokens
            do
            {
                tokenFound = false;
                foreach (var nextToken in _tokens.Where(item => ignoreTokens.Any(t => t == item.Type)))
                {
                    tokenFound = nextToken.Match(_content, _offset, out _handle);
                    if (tokenFound)
                    {
                        Consume();
                        break;
                    }
                }

                if (this.IsEof()) return false;
            }
            while (tokenFound);

            // Read requested token
            foreach (var nextToken in _tokens.Where(item => requestedTokens.Any(t => t == item.Type)))
            {
                tokenFound = nextToken.Match(_content, _offset, out _handle);
                if (tokenFound)
                {
                    token = nextToken.Type;
                    value = _handle.Value;
                    break;
                }
            }

            return tokenFound;
        }

        /// <summary>
        /// The consume.
        /// </summary>
        public void Consume()
        {
            if (_handle != null) _offset = _handle.Consume();
        }

        /// <summary>
        /// The is EOF.
        /// </summary>
        private bool IsEof()
        {
            return _offset >= _content.Length;
        }
    }
}
