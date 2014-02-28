using System;
using System.Collections.Generic;
using System.Linq;

namespace OGS.HOCON.Impl
{
    internal class Tokenizer
    {
        private readonly List<Token> _tokens;
        private TokenHandle _handle;
        private string _content;
        private int _offset;

        public int Offset
        {
            get { return _offset; }
        }

        public Tokenizer(string content, IEnumerable<Token> tokens)
        {
            _tokens = new List<Token>(tokens);
            _content = content;
        }

        public void Include(string content)
        {
            _content = _content.Insert(_offset, Environment.NewLine + content + Environment.NewLine);
        }

        public bool ReadNext(out TokenType token, out string value, TokenType[] requestedTokens, TokenType[] ignoreTokens)
        {
            token = TokenType.Unknown;
            value = string.Empty;

            if (IsEOF()) return false;

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

                if (IsEOF()) return false;
            } while (tokenFound);

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

        public void Consume()
        {
            if (_handle != null) _offset = _handle.Consume();
        }

        private bool IsEOF()
        {
            return _offset >= _content.Length;
        }
    }
}