using System.Collections.Generic;

namespace PrototextSharp
{
    /// <summary>
    /// Represents a prototext message as a set of named fields and their values.
    /// </summary>
    /// <remarks>
    /// Field names are case-sensitive. Repeated occurrences are retained in insertion order,
    /// and a field may contain scalar values, nested <see cref="ProtoNode"/> instances, or both.
    /// </remarks>
    public class ProtoNode
    {
        /// <summary>
        /// Gets the values for each field, keyed by case-sensitive prototext field name.
        /// </summary>
        /// <remarks>
        /// Each field maps to a list so repeated values and their insertion order are preserved.
        /// The returned dictionary and lists are mutable.
        /// </remarks>
        public Dictionary<string, List<object?>> Fields { get; }

        /// <summary>
        /// Initializes an empty prototext message node.
        /// </summary>
        public ProtoNode ()
        {
            Fields = new Dictionary<string, List<object?>>();
        }

        /// <summary>
        /// Adds a value to a field, appending it after any existing values.
        /// </summary>
        /// <param name="key">The case-sensitive prototext field name.</param>
        /// <param name="value">The scalar value or child node to add.</param>
        /// <remarks>
        /// The key is used as supplied and is compared case-sensitively. A <see langword="null"/>
        /// value can be stored, although the parameter is declared as non-nullable.
        /// </remarks>
        public void Add(string key, object value)
        {
            if (!Fields.TryGetValue(key, out List<object?> values))
            {
                values = new List<object?>();
                Fields[key] = values;
            }

            values.Add(value);
        }

        /// <summary>
        /// Gets the value or values associated with a field name.
        /// </summary>
        /// <param name="key">The case-sensitive prototext field name.</param>
        /// <returns>
        /// <see langword="null"/> when the field is absent; otherwise, the sole value when the field
        /// occurs once, or the list of values in insertion order when it occurs more than once.
        /// </returns>
        /// <remarks>
        /// A stored single <see langword="null"/> value and an absent field both return
        /// <see langword="null"/>. Use <see cref="Fields"/> to distinguish these cases.
        /// </remarks>
        public object? this[string key]
        {
            get
            {
                if (!Fields.TryGetValue(key, out List<object?> values))
                    return null;

                return values.Count == 1 ? values[0] : values;
            }
        }
    }
}
