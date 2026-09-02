using System.Globalization;

namespace PrototextSharp.Tests;

public class PrototextWriterTests
{
    private enum WriterState
    {
        Ready
    }

    /// <summary>
    /// Verifies that serializing a null root node produces an argument-null error with the correct parameter name.
    /// </summary>
    [Test]
    public void Serialize_NullNode_ThrowsArgumentNullException()
    {
        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(
            () => new PrototextWriter().Serialize(null!));

        Assert.That(exception!.ParamName, Is.EqualTo("node"));
    }

    /// <summary>
    /// Verifies that a node containing no fields serializes to an empty document.
    /// </summary>
    [Test]
    public void Serialize_EmptyNode_ReturnsEmptyString()
    {
        string result = new PrototextWriter().Serialize(new ProtoNode());

        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// Verifies the default formatting of a single scalar field.
    /// </summary>
    [Test]
    public void Serialize_SingleStringField_UsesDefaultFormatting()
    {
        var node = new ProtoNode();
        node.Add("name", "Lumentum");

        string result = new PrototextWriter().Serialize(node);

        Assert.That(result, Is.EqualTo(Lines("name: \"Lumentum\"")));
    }

    /// <summary>
    /// Verifies that every supported scalar CLR type uses its expected prototext representation.
    /// </summary>
    [Test]
    public void Serialize_AllSupportedScalarTypes_UsesExpectedRepresentations()
    {
        var node = new ProtoNode();
        node.Add("text", "value");
        node.Add("true_value", true);
        node.Add("false_value", false);
        node.Add("byte_value", (byte)1);
        node.Add("sbyte_value", (sbyte)-2);
        node.Add("short_value", (short)-3);
        node.Add("ushort_value", (ushort)4);
        node.Add("int_value", -5);
        node.Add("uint_value", 6U);
        node.Add("long_value", -7L);
        node.Add("ulong_value", 8UL);
        node.Add("float_value", 1.5F);
        node.Add("double_value", -2.25D);
        node.Add("decimal_value", 3.75M);
        node.Add("enum_value", WriterState.Ready);
        node.Add("null_value", null!);

        string result = new PrototextWriter().Serialize(node);

        Assert.That(result, Is.EqualTo(Lines(
            "text: \"value\"",
            "true_value: true",
            "false_value: false",
            "byte_value: 1",
            "sbyte_value: -2",
            "short_value: -3",
            "ushort_value: 4",
            "int_value: -5",
            "uint_value: 6",
            "long_value: -7",
            "ulong_value: 8",
            "float_value: 1.5",
            "double_value: -2.25",
            "decimal_value: 3.75",
            "enum_value: Ready",
            "null_value: null")));
    }

    /// <summary>
    /// Verifies that characters with special meaning in quoted strings are escaped in the correct order.
    /// </summary>
    [Test]
    public void Serialize_StringValue_EscapesSpecialCharacters()
    {
        var node = new ProtoNode();
        node.Add("value", "slash=\\ quote=\" return=\r newline=\n tab=\t");

        string result = new PrototextWriter().Serialize(node);

        Assert.That(result, Is.EqualTo(Lines("value: \"slash=\\\\ quote=\\\" return=\\r newline=\\n tab=\\t\"")));
    }

    /// <summary>
    /// Verifies that nested messages use the default indentation and balanced message braces.
    /// </summary>
    [Test]
    public void Serialize_NestedMessages_UsesDefaultIndentation()
    {
        var inner = new ProtoNode();
        inner.Add("value", "found");
        var middle = new ProtoNode();
        middle.Add("inner", inner);
        var root = new ProtoNode();
        root.Add("outer", middle);

        string result = new PrototextWriter().Serialize(root);

        Assert.That(result, Is.EqualTo(Lines(
            "outer {",
            " inner {",
            "  value: \"found\"",
            " }",
            "}")));
    }

    /// <summary>
    /// Verifies that custom indentation text is repeated once for every nesting level.
    /// </summary>
    [Test]
    public void Serialize_CustomIndentation_RepeatsForEveryLayer()
    {
        var grandchild = new ProtoNode();
        grandchild.Add("value", 1L);
        var child = new ProtoNode();
        child.Add("grandchild", grandchild);
        var root = new ProtoNode();
        root.Add("child", child);

        string result = new PrototextWriter("--").Serialize(root);

        Assert.That(result, Is.EqualTo(Lines(
            "child {",
            "--grandchild {",
            "----value: 1",
            "--}",
            "}")));
    }

    /// <summary>
    /// Verifies that empty indentation text produces nested output without leading whitespace.
    /// </summary>
    [Test]
    public void Serialize_EmptyIndentation_DisablesIndentation()
    {
        var child = new ProtoNode();
        child.Add("value", 1L);
        var root = new ProtoNode();
        root.Add("child", child);

        string result = new PrototextWriter(string.Empty).Serialize(root);

        Assert.That(result, Is.EqualTo(Lines(
            "child {",
            "value: 1",
            "}")));
    }

    /// <summary>
    /// Verifies that an empty child node is emitted as an empty message block.
    /// </summary>
    [Test]
    public void Serialize_EmptyChildNode_WritesEmptyMessageBlock()
    {
        var root = new ProtoNode();
        root.Add("child", new ProtoNode());

        string result = new PrototextWriter().Serialize(root);

        Assert.That(result, Is.EqualTo(Lines(
            "child {",
            "}")));
    }

    /// <summary>
    /// Verifies that repeated scalar and message fields retain their insertion order.
    /// </summary>
    [Test]
    public void Serialize_RepeatedFields_PreservesInsertionOrder()
    {
        var child = new ProtoNode();
        child.Add("id", 3L);
        var root = new ProtoNode();
        root.Add("item", 1L);
        root.Add("item", 2L);
        root.Add("item", child);

        string result = new PrototextWriter().Serialize(root);

        Assert.That(result, Is.EqualTo(Lines(
            "item: 1",
            "item: 2",
            "item {",
            " id: 3",
            "}")));
    }

    /// <summary>
    /// Verifies that distinct fields retain the order in which they were added to the node.
    /// </summary>
    [Test]
    public void Serialize_DistinctFields_PreservesInsertionOrder()
    {
        var node = new ProtoNode();
        node.Add("third", 3L);
        node.Add("first", 1L);
        node.Add("second", 2L);

        string result = new PrototextWriter().Serialize(node);

        Assert.That(result, Is.EqualTo(Lines(
            "third: 3",
            "first: 1",
            "second: 2")));
    }

    /// <summary>
    /// Verifies that numeric output uses invariant formatting when the current culture uses decimal commas.
    /// </summary>
    [Test]
    public void Serialize_NumericValues_UsesInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-CA");
            var node = new ProtoNode();
            node.Add("double_value", 1234.5D);
            node.Add("decimal_value", 6789.25M);

            string result = new PrototextWriter().Serialize(node);

            Assert.That(result, Is.EqualTo(Lines(
                "double_value: 1234.5",
                "decimal_value: 6789.25")));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    /// <summary>
    /// Verifies the writer's representations of non-finite floating-point values.
    /// </summary>
    [Test]
    public void Serialize_NonFiniteFloatingPointValues_UsesRuntimeIdentifiers()
    {
        var node = new ProtoNode();
        node.Add("not_a_number", double.NaN);
        node.Add("positive_infinity", double.PositiveInfinity);
        node.Add("negative_infinity", double.NegativeInfinity);

        string result = new PrototextWriter().Serialize(node);

        Assert.That(result, Is.EqualTo(Lines(
            "not_a_number: NaN",
            "positive_infinity: Infinity",
            "negative_infinity: -Infinity")));
    }

    /// <summary>
    /// Verifies that an unsupported scalar type produces an error identifying that type.
    /// </summary>
    [Test]
    public void Serialize_UnsupportedValueType_ThrowsNotSupportedException()
    {
        var node = new ProtoNode();
        node.Add("version", new Version(1, 2));

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(
            () => new PrototextWriter().Serialize(node));

        Assert.That(exception!.Message, Does.Contain(typeof(Version).FullName));
    }

    /// <summary>
    /// Verifies that writer output containing supported parser values can be parsed back into an equivalent hierarchy.
    /// </summary>
    [Test]
    public void Serialize_SupportedHierarchy_CanBeParsedAgain()
    {
        var child = new ProtoNode();
        child.Add("id", 42L);
        var root = new ProtoNode();
        root.Add("name", "quoted \"value\"");
        root.Add("enabled", true);
        root.Add("child", child);

        string prototext = new PrototextWriter("  ").Serialize(root);
        ProtoNode parsed = new PrototextParser().Parse(prototext);
        var parsedChild = (ProtoNode)parsed["child"]!;

        Assert.Multiple(() =>
        {
            Assert.That(parsed["name"], Is.EqualTo("quoted \"value\""));
            Assert.That(parsed["enabled"], Is.EqualTo(true));
            Assert.That(parsedChild["id"], Is.EqualTo(42L));
        });
    }

    /// <summary>
    /// Joins output lines using the current platform line ending and adds the writer's final line ending.
    /// </summary>
    /// <param name="lines">The expected output lines.</param>
    /// <returns>The lines formatted as a complete writer result.</returns>
    private static string Lines(params string[] lines)
    {
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}
