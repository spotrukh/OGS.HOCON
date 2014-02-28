using System.Text.RegularExpressions;

namespace OGS.HOCON.Impl
{
    internal class Stop
    {
        public Regex Parser { get; private set; }
        
        public Stop(string parser)
        {
            Parser = new Regex(parser, RegexOptions.Compiled);
        }
        
        public bool Match(string content, int offset)
        {
            var match = Parser.Match(content, offset);
            return match.Success && match.Groups[0].Index == offset;
        }
    }

    internal class Token
    {
        public Regex Parser { get; private set; }
        public TokenType Type { get; private set; }
        public Stop Stop { get; private set; }

        public Token(TokenType type, string parser, Stop stop = null)
        {
            Parser = new Regex(parser, RegexOptions.Compiled);
            Type = type;
            Stop = stop;
        }
        
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