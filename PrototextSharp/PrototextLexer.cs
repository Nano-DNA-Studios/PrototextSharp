using System;
using System.Text;
using NLog;

namespace PrototextSharp
{
    /// <summary>
    /// Converts prototext input into the token stream consumed by <see cref="PrototextParser"/>.
    /// </summary>
    internal class PrototextLexer
    {
        /// <summary>
        /// Writes lexer diagnostics through the NLog configuration supplied by the consuming application.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The complete prototext input being tokenized.
        /// </summary>
        private readonly string _text;

        /// <summary>
        /// The zero-based index of the next input character to process.
        /// </summary>
        private int _position;

        /// <summary>
        /// Initializes a lexer for the supplied prototext input.
        /// </summary>
        /// <param name="text">The prototext content to tokenize.</param>
        public PrototextLexer(string text)
        {
            _text = text;
            _position = 0;
        }

        /// <summary>
        /// Reads the next token from the input after skipping whitespace and comments.
        /// </summary>
        /// <returns>The next token, or an end token when all input has been consumed.</returns>
        /// <exception cref="FormatException">Thrown when an unexpected character or unterminated string is encountered.</exception>
        public Token NextToken()
        {
            SkipWhitespacesAndComments();

            if (_position >= _text.Length)
                return new Token(TokenType.End, "");

            char c = _text[_position];

            if (c == ':')
            {
                _position++;
                return new Token(TokenType.Colon, ":");
            }

            if (c == '{')
            {
                _position++;
                return new Token(TokenType.OpenBrace, "{");
            }

            if (c == '}')
            {
                _position++;
                return new Token(TokenType.CloseBrace, "}");
            }

            if (c == '"')
                return ReadString();

            if (char.IsDigit(c) || c == '-')
                return ReadNumber();

            if (IsIdentifierStart(c))
                return ReadIdentifierOrBoolean();

            Logger.Debug("Lexing failed at position {0} due to an unexpected character.", _position);
            throw new FormatException($"Unexpected character '{c}' at position {_position}.");
        }

        /// <summary>
        /// Advances past whitespace, hash comments, and double-slash comments.
        /// </summary>
        private void SkipWhitespacesAndComments()
        {
            while (_position < _text.Length)
            {
                if (char.IsWhiteSpace(_text[_position]))
                {
                    _position++;
                    continue;
                }

                if (_text[_position] == '#')
                {
                    SkipLine();
                    continue;
                }

                if (_position + 1 < _text.Length && _text[_position] == '/' && _text[_position + 1] == '/')
                {
                    SkipLine();
                    continue;
                }

                break;
            }
        }

        /// <summary>
        /// Advances to the end of the current line or the end of the input.
        /// </summary>
        private void SkipLine()
        {
            while (_position < _text.Length && _text[_position] != '\n')
                _position++;
        }

        /// <summary>
        /// Reads a quoted string value and decodes its escape sequences.
        /// </summary>
        /// <returns>A string token containing the decoded value.</returns>
        /// <exception cref="FormatException">Thrown when the quoted string is not terminated.</exception>
        private Token ReadString()
        {
            _position++;
            StringBuilder builder = new StringBuilder();

            while (_position < _text.Length)
            {
                char c = _text[_position++];

                if (c == '"')
                    return new Token(TokenType.String, builder.ToString());

                if (c == '\\' && _position < _text.Length)
                {
                    char escaped = _text[_position++];

                    builder.Append(GetEscapeChar(escaped));

                    continue;
                }

                builder.Append(c);
            }

            Logger.Debug("Lexing failed at position {0} due to an unterminated string literal.", _position);
            throw new FormatException("Unterminated string literal.");
        }

        /// <summary>
        /// Reads a signed or unsigned integer or decimal token.
        /// </summary>
        /// <returns>A number token containing the original numeric text.</returns>
        private Token ReadNumber()
        {
            int start = _position;

            if (_text[_position] == '-')
                _position++;

            while (_position < _text.Length && char.IsDigit(_text[_position]))
                _position++;

            if (_position < _text.Length && _text[_position] == '.')
            {
                _position++;

                while (_position < _text.Length && char.IsDigit(_text[_position]))
                    _position++;
            }

            return new Token(TokenType.Number, _text[start.._position]);
        }

        /// <summary>
        /// Converts a string escape designator into the character it represents.
        /// </summary>
        /// <param name="input">The character following a backslash.</param>
        /// <returns>The decoded escape character, or the input character when the escape is unrecognized.</returns>
        private char GetEscapeChar(char input)
        {
            switch (input)
            {
                case '"': return '"';
                case 'n': return '\n';
                case 'r': return '\r';
                case 't': return '\t';
                case '\\': return '\\';
                default: return input;
            }
        }

        /// <summary>
        /// Reads an identifier and classifies the lowercase Boolean literals separately.
        /// </summary>
        /// <returns>An identifier token or a Boolean token.</returns>
        private Token ReadIdentifierOrBoolean()
        {
            int start = _position;

            while (_position < _text.Length && IsIdentifierPart(_text[_position]))
                _position++;

            string value = _text[start.._position];

            if (value == "true" || value == "false")
                return new Token(TokenType.Boolean, value);

            return new Token(TokenType.Identifier, value);
        }

        /// <summary>
        /// Determines whether a character can begin an identifier.
        /// </summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns><see langword="true"/> when the character is a letter or underscore; otherwise, <see langword="false"/>.</returns>
        private static bool IsIdentifierStart(char c)
        {
            return char.IsLetter(c) || c == '_';
        }

        /// <summary>
        /// Determines whether a character can appear after the first character of an identifier.
        /// </summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns><see langword="true"/> when the character is supported in an identifier; otherwise, <see langword="false"/>.</returns>
        private static bool IsIdentifierPart(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == '-';
        }
    }
}

