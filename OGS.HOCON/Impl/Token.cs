namespace OGS.HOCON.Impl
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// The token.
    /// </summary>
    internal class Token
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        public Token(TokenType type, string parser, Stop stop = null)
        {
            Parser = new Regex(parser, RegexOptions.Compiled);
            Type = type;
            Stop = stop;
        }

        /// <summary>
        /// Gets the parser.
        /// </summary>
        public Regex Parser { get; private set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public TokenType Type { get; private set; }

        /// <summary>
        /// Gets the stop.
        /// </summary>
        public Stop Stop { get; private set; }

        /// <summary>
        /// The match.
        /// </summary>
        public bool Match(string content, int offset, out TokenHandle handle)
        {
            handle = null;

            var match = Parser.Match(content, offset);
            if (match.Success == false)
                return false;

            var capture = match.Captures[0];
            if (capture.Index != offset)
                return false;

            if (Stop != null && Stop.Match(content, match.Groups[0].Index + match.Groups[0].Length) == false)
                return false;
                
            if (match.Groups["value"].Success)
                handle = new TokenHandle(match.Groups["value"].Value, offset + capture.Length);
            else
                handle = new TokenHandle(capture.Value, offset + capture.Length);

            return true;
        }
    }
}
