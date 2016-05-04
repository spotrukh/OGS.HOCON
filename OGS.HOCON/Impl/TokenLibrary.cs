namespace OGS.HOCON.Impl
{
    using System.Collections.Generic;

    /// <summary>
    /// The token library.
    /// </summary>
    internal static class TokenLibrary
    {
        /// <summary>
        /// The tokens.
        /// </summary>
        public static readonly IEnumerable<Token> Tokens;

        /// <summary>
        /// Initializes static members of the <see cref="TokenLibrary"/> class.
        /// </summary>
        static TokenLibrary()
        {
            Tokens = new List<Token>
                {
                    new Token(TokenType.Comment, @"([#]|(//)).*([\n\r])?"),
                    new Token(TokenType.Include, @"include[ ]*[""](?<value>.+)[""]"),

                    new Token(TokenType.Key, @"[a-zA-Z0-9_-]+([.][a-zA-Z0-9_]+)*"),
                    new Token(TokenType.Space, @"[ \r\n\t]+"),

                    new Token(TokenType.Assign, @"[(:)|(=)]"),

                    new Token(TokenType.BeginArray, @"[[]"),
                    new Token(TokenType.EndArray, @"[]]"),
                    new Token(TokenType.ArraySeparator, @"[,]"),

                    new Token(TokenType.Substitution, @"[$][(](?<value>(\w|[._-])+)[)]"),
                    new Token(TokenType.Substitution, @"[$][{](?<value>(\w|[._-])+)[}]"),
                    new Token(TokenType.SafeSubstitution, @"[$][(][?](?<value>\w*)[)]"),
                    new Token(TokenType.SafeSubstitution, @"[$][{][?](?<value>\w*)[}]"),
                    new Token(TokenType.BooleanValue, @"(?i:(on|off|true|false|yes|no|enabled|disabled))", new Stop(@"\Z|[ \],\r\n\t]")),
                    new Token(TokenType.DeciamlValue, @"(?<value>[-]?[0-9]+[.][0-9]+)", new Stop(@"\Z|[ \],\r\n\t]")),
                    new Token(TokenType.DoubleValue, @"(?<value>[-]?[0-9]+[Ee][0-9]+)", new Stop(@"\Z|[ \],\r\n\t]")),
                    new Token(TokenType.NumericValue, @"(?<value>[-]?[0-9]+)", new Stop(@"\Z|[ \],\r\n\t]")),
                    new Token(TokenType.StringValue, @"[""](?<value>([""][""]|[^""])*)[""]", new Stop(@"\Z|[ \],\r\n\t]")),
                    new Token(TokenType.StringValue, @"([^""${}\[\]:=,+#'^?!@*& \r\n\t])+", new Stop(@"\Z|[ \],\r\n\t]")),

                    new Token(TokenType.BeginScope, @"[{]"),
                    new Token(TokenType.EndScope, @"[}]")
                };
        }
    }
}