
namespace PrototextSharp
{
    /// <summary>
    /// Identifies the lexical token categories supported by the prototext parser.
    /// </summary>
    internal enum TokenType
    {
        /// <summary>
        /// A field name or unquoted scalar identifier.
        /// </summary>
        Identifier,

        /// <summary>
        /// A decoded quoted string value.
        /// </summary>
        String,

        /// <summary>
        /// An integer or decimal numeric value.
        /// </summary>
        Number,

        /// <summary>
        /// A lowercase true or false value.
        /// </summary>
        Boolean,

        /// <summary>
        /// The separator between a field name and scalar value.
        /// </summary>
        Colon,

        /// <summary>
        /// The beginning of a nested message.
        /// </summary>
        OpenBrace,

        /// <summary>
        /// The end of a nested message.
        /// </summary>
        CloseBrace,

        /// <summary>
        /// The end of the input stream.
        /// </summary>
        End
    }
}
