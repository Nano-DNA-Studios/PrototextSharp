using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NLog;

namespace PrototextSharp
{
    /// <summary>
    /// Serializes <see cref="ProtoNode"/> hierarchies into prototext text.
    /// </summary>
    /// <remarks>
    /// The writer preserves field and repeated-value enumeration order. Its supported scalar
    /// values are strings, Booleans, numeric CLR types, enums, and <see langword="null"/>.
    /// </remarks>
    public class PrototextWriter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The text repeated once for each indentation level.
        /// </summary>
        private readonly string _indentText;

        /// <summary>
        /// Initializes a new writer with the text used at each indentation level.
        /// </summary>
        /// <param name="indentText">The text to emit for each indentation level.</param>
        /// <remarks>A null indentation value is not validated by the constructor.</remarks>
        public PrototextWriter(string indentText = " ")
        {
            _indentText = indentText;
        }

        /// <summary>
        /// Serializes a complete <see cref="ProtoNode"/> hierarchy into prototext.
        /// </summary>
        /// <param name="node">The root node to serialize.</param>
        /// <returns>A prototext document representing the supplied node hierarchy.</returns>
        /// <remarks>The returned document ends each emitted line with the platform newline.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
        /// <exception cref="NotSupportedException">Thrown when a field contains a value type that cannot be serialized.</exception>
        public string Serialize(ProtoNode node)
        {
            Logger.Debug("Starting prototext serialization.");

            if (node == null)
            {
                Logger.Error("The provided Node cannot be null");
                throw new ArgumentNullException(nameof(node));
            }

            StringBuilder content = new StringBuilder();
            SerializeNode(node, content, 0);

            string result = content.ToString();
            Logger.Debug("Completed prototext serialization with {0} root fields.", node.Fields.Count);
            return result;
        }

        /// <summary>
        /// Appends every field in a node to the output buffer at the specified nesting level.
        /// </summary>
        /// <param name="node">The node whose fields will be serialized.</param>
        /// <param name="content">The output buffer receiving the serialized fields.</param>
        /// <param name="layer">The zero-based indentation level of the node.</param>
        private void SerializeNode(ProtoNode node, StringBuilder content, int layer)
        {
            foreach ((string key, List<object?> values) in node.Fields)
            {
                foreach (object? value in values)
                    SerializeField(key, value, content, layer);
            }
        }

        /// <summary>
        /// Appends one scalar or nested-message field to the output buffer.
        /// </summary>
        /// <param name="key">The prototext field name.</param>
        /// <param name="value">The scalar value or child node to serialize.</param>
        /// <param name="content">The output buffer receiving the serialized field.</param>
        /// <param name="layer">The zero-based indentation level of the field.</param>
        private void SerializeField(string key, object? value, StringBuilder content, int layer)
        {
            string indent = GetIndent(layer);

            if (value is ProtoNode childNode)
            {
                content.Append(indent);
                content.Append(key);
                content.AppendLine(" {");

                SerializeNode(childNode, content, layer + 1);

                content.Append(indent);
                content.AppendLine("}");

                return;
            }

            content.Append(indent);
            content.Append(key);
            content.Append(": ");
            content.AppendLine(SerializeValue(value));
        }

        /// <summary>
        /// Converts a supported scalar value into its invariant prototext representation.
        /// </summary>
        /// <param name="value">The scalar value to serialize.</param>
        /// <returns>The serialized scalar value.</returns>
        /// <exception cref="NotSupportedException">Thrown when the value type is not supported.</exception>
        private static string SerializeValue(object? value)
        {
            return value switch
            {
                null => "null",

                string text =>
                    $"\"{EscapeString(text)}\"",

                bool boolean =>
                    boolean ? "true" : "false",

                byte number =>
                    number.ToString(CultureInfo.InvariantCulture),

                sbyte number =>
                    number.ToString(CultureInfo.InvariantCulture),

                short number =>
                    number.ToString(CultureInfo.InvariantCulture),

                ushort number =>
                    number.ToString(CultureInfo.InvariantCulture),

                int number =>
                    number.ToString(CultureInfo.InvariantCulture),

                uint number =>
                    number.ToString(CultureInfo.InvariantCulture),

                long number =>
                    number.ToString(CultureInfo.InvariantCulture),

                ulong number =>
                    number.ToString(CultureInfo.InvariantCulture),

                float number =>
                    number.ToString(CultureInfo.InvariantCulture),

                double number =>
                    number.ToString(CultureInfo.InvariantCulture),

                decimal number =>
                    number.ToString(CultureInfo.InvariantCulture),

                Enum enumValue =>
                    enumValue.ToString(),

                _ => throw LogUnsupportedValue(value)
            };
        }

        /// <summary>
        /// Creates and logs the exception raised when a value has an unsupported type.
        /// </summary>
        /// <param name="value">The non-null value that cannot be serialized.</param>
        /// <returns>The logged exception to throw.</returns>
        /// <remarks>
        /// The value itself is not logged; only its type name is included in the exception message.
        /// </remarks>
        private static NotSupportedException LogUnsupportedValue(object value)
        {
            NotSupportedException exception = new NotSupportedException(
                $"Cannot serialize values of type '{value.GetType().FullName}'.");

            Logger.Error(exception, "Cannot serialize a value with an unsupported type.");
            return exception;
        }

        /// <summary>
        /// Escapes characters that have special meaning inside a quoted prototext string.
        /// </summary>
        /// <param name="value">The unescaped string value.</param>
        /// <returns>The value with backslashes, quotes, carriage returns, line feeds, and tabs escaped.</returns>
        private static string EscapeString(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

        /// <summary>
        /// Builds the indentation prefix for a nesting level.
        /// </summary>
        /// <param name="layer">The zero-based indentation level.</param>
        /// <returns>The configured indentation text repeated once for each level.</returns>
        private string GetIndent(int layer) => string.Concat(Enumerable.Repeat(_indentText, layer));
    }
}
