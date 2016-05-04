namespace OGS.HOCON.Impl
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// The stop.
    /// </summary>
    internal class Stop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Stop"/> class.
        /// </summary>
        /// <param name="parser">
        /// The parser.
        /// </param>
        public Stop(string parser)
        {
            this.Parser = new Regex(parser, RegexOptions.Compiled);
        }

        /// <summary>
        /// Gets the parser.
        /// </summary>
        public Regex Parser { get; private set; }

        /// <summary>
        /// The match.
        /// </summary>
        public bool Match(string content, int offset)
        {
            var match = this.Parser.Match(content, offset);
            return match.Success && match.Groups[0].Index == offset;
        }
    }
}
