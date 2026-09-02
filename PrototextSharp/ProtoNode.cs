using System.Collections.Generic;

namespace PrototextSharp
{
    /// <summary>
    /// Represents a prototext message and its scalar or nested field values.
    /// </summary>
    public class ProtoNode
    {
        /// <summary>
        /// Gets the field values keyed by their case-sensitive prototext field names.
        /// </summary>
        public Dictionary<string, List<object?>> Fields { get; }

        /// <summary>
        /// Initializes an empty prototext message node.
        /// </summary>
        public ProtoNode ()
        {
            Fields = new Dictionary<string, List<object?>>();
        }

        /// <summary>
        /// Adds a value to a field while retaining repeated values in insertion order.
        /// </summary>
        /// <param name="key">The case-sensitive prototext field name.</param>
        /// <param name="value">The scalar value or child node to add.</param>
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
        /// Gets a field value, returning the value directly for a single occurrence or a list for repeated occurrences.
        /// </summary>
        /// <param name="key">The case-sensitive prototext field name.</param>
        /// <returns>The single value, the list of repeated values, or null when the field does not exist.</returns>
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
