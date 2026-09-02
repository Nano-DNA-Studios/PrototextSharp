using System;
using System.Globalization;

namespace PrototextSharp
{
    /// <summary>
    /// Parses prototext input into a hierarchy of <see cref="ProtoNode"/> instances.
    /// </summary>
    public class PrototextParser
    {
        private PrototextLexer _lexer = null!;

        private Token _current = null!;

        /// <summary>
        /// Parses a complete prototext document.
        /// </summary>
        /// <param name="text">The prototext content to parse.</param>
        /// <returns>The root node containing the fields parsed from the document.</returns>
        /// <exception cref="FormatException">Thrown when the input contains invalid prototext syntax or values.</exception>
        /// <exception cref="OverflowException">Thrown when an integer value is outside the range of <see cref="long"/>.</exception>
        public ProtoNode Parse(string text)
        {
            _lexer = new PrototextLexer(text);
            _current = _lexer.NextToken();

            ProtoNode root = ParseMessage();

            Expect(TokenType.End);

            return root;
        }

        /// <summary>
        /// Parses fields until the end of the input or the closing brace of the current message is reached.
        /// </summary>
        /// <returns>A node containing the fields of the parsed message.</returns>
        private ProtoNode ParseMessage()
        {
            ProtoNode node = new ProtoNode();

            while (_current.Type != TokenType.End && _current.Type != TokenType.CloseBrace)
            {
                string fieldName = Expect(TokenType.Identifier).Value;

                if (_current.Type == TokenType.OpenBrace)
                {
                    Advance();

                    ProtoNode child = ParseMessage();

                    Expect(TokenType.CloseBrace);

                    node.Add(fieldName, child);

                }
                else
                {
                    Expect(TokenType.Colon);

                    object? value = ParseValue();

                    node.Add(fieldName, value);
                }
            }

            return node;
        }

        /// <summary>
        /// Parses the current scalar token into its corresponding CLR value.
        /// </summary>
        /// <returns>The parsed string, Boolean, integer, decimal, or identifier value.</returns>
        /// <exception cref="FormatException">Thrown when the current token cannot be used as a scalar value.</exception>
        private object? ParseValue()
        {
            Token token = _current;

            switch (token.Type)
            {
                case TokenType.String:
                    Advance();
                    return token.Value;

                case TokenType.Boolean:
                    Advance();
                    return bool.Parse(token.Value);

                case TokenType.Number:
                    Advance();

                    if (token.Value.Contains('.'))
                        return double.Parse(token.Value, CultureInfo.InvariantCulture);

                    return long.Parse(token.Value, CultureInfo.InvariantCulture);
                case TokenType.Identifier:
                    Advance();
                    return token.Value;
                default:
                    throw new FormatException($"Expected value but found {token.Type}.");
            }
        }

        /// <summary>
        /// Validates that the current token has the expected type and advances to the next token.
        /// </summary>
        /// <param name="type">The token type required at the current parser position.</param>
        /// <returns>The validated token.</returns>
        /// <exception cref="FormatException">Thrown when the current token does not have the expected type.</exception>
        private Token Expect(TokenType type)
        {
            if (_current.Type != type)
                throw new FormatException($"Expected {type} but found {_current.Type}.");

            Token token = _current;
            Advance();

            return token;
        }

        /// <summary>
        /// Advances the parser to the next token produced by the lexer.
        /// </summary>
        private void Advance()
        {
            _current = _lexer.NextToken();
        }
    }
}
