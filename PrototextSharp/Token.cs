
namespace PrototextSharp
{
    /// <summary>
    /// Represents one lexical token and its source value.
    /// </summary>
    internal class Token
    {
        /// <summary>
        /// Gets the lexical category of the token.
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// Gets the text or decoded value associated with the token.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Initializes a token with its category and value.
        /// </summary>
        /// <param name="type">The lexical category of the token.</param>
        /// <param name="value">The text or decoded value associated with the token.</param>
        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}
