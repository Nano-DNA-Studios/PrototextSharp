using System.Globalization;

namespace PrototextSharp.Tests;

/// <summary>
/// Verifies successful parsing of supported prototext values and message structures.
/// </summary>
public class PrototextParserTests
{
    private PrototextParser _parser = null!;

    /// <summary>
    /// Creates a fresh parser before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _parser = new PrototextParser();
    }

    /// <summary>
    /// Verifies that input containing no fields produces an empty root node.
    /// </summary>
    /// <param name="input">The empty or whitespace-only prototext to parse.</param>
    [TestCase("")]
    [TestCase("   \t\r\n")]
    [TestCase("\u2003\u00a0")]
    public void Parse_WhenInputHasNoFields_ReturnsEmptyNode(string input)
    {
        ProtoNode result = _parser.Parse(input);

        Assert.That(result.Fields, Is.Empty);
    }

    /// <summary>
    /// Verifies that strings, integers, decimals, booleans, and identifiers are parsed into the expected CLR types.
    /// </summary>
    [Test]
    public void Parse_ParsesAllSupportedScalarTypes()
    {
        const string input = "text: \"value\" integer: 42 negative: -17 decimal: 3.25 negative_decimal: -.5 enabled: true disabled: false mode: BOOT_MODE_INSECURE";

        ProtoNode result = _parser.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(result["text"], Is.EqualTo("value").And.TypeOf<string>());
            Assert.That(result["integer"], Is.EqualTo(42L).And.TypeOf<long>());
            Assert.That(result["negative"], Is.EqualTo(-17L).And.TypeOf<long>());
            Assert.That(result["decimal"], Is.EqualTo(3.25d).And.TypeOf<double>());
            Assert.That(result["negative_decimal"], Is.EqualTo(-0.5d).And.TypeOf<double>());
            Assert.That(result["enabled"], Is.EqualTo(true).And.TypeOf<bool>());
            Assert.That(result["disabled"], Is.EqualTo(false).And.TypeOf<bool>());
            Assert.That(result["mode"], Is.EqualTo("BOOT_MODE_INSECURE").And.TypeOf<string>());
        });
    }

    /// <summary>
    /// Verifies that a decimal ending with a decimal point is parsed successfully.
    /// </summary>
    [Test]
    public void Parse_ParsesTrailingDecimalPoint()
    {
        ProtoNode result = _parser.Parse("value: 1.");

        Assert.That(result["value"], Is.EqualTo(1d).And.TypeOf<double>());
    }

    /// <summary>
    /// Verifies that empty messages and deeply nested messages produce the correct node hierarchy.
    /// </summary>
    [Test]
    public void Parse_ParsesEmptyAndDeeplyNestedMessages()
    {
        const string input = "empty {} outer { middle { inner { value: \"found\" } } }";

        ProtoNode result = _parser.Parse(input);

        var empty = (ProtoNode)result["empty"]!;
        var outer = (ProtoNode)result["outer"]!;
        var middle = (ProtoNode)outer["middle"]!;
        var inner = (ProtoNode)middle["inner"]!;

        Assert.Multiple(() =>
        {
            Assert.That(empty.Fields, Is.Empty);
            Assert.That(inner["value"], Is.EqualTo("found"));
        });
    }

    /// <summary>
    /// Verifies that repeated scalar fields are returned as a list in source order.
    /// </summary>
    [Test]
    public void Parse_RepeatedScalarFields_ReturnsValuesInSourceOrder()
    {
        ProtoNode result = _parser.Parse("item: \"first\" item: \"second\" item: \"third\"");

        var values = (List<object?>)result["item"]!;

        Assert.That(values, Is.EqualTo(new object?[] { "first", "second", "third" }));
    }

    /// <summary>
    /// Verifies that repeated message fields are returned as child nodes in source order.
    /// </summary>
    [Test]
    public void Parse_RepeatedMessageFields_ReturnsNodesInSourceOrder()
    {
        ProtoNode result = _parser.Parse("item { id: 1 } item { id: 2 }");

        var values = ((List<object?>)result["item"]!).Cast<ProtoNode>().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(values, Has.Count.EqualTo(2));
            Assert.That(values[0]["id"], Is.EqualTo(1L));
            Assert.That(values[1]["id"], Is.EqualTo(2L));
        });
    }

    /// <summary>
    /// Verifies that repeated occurrences of a field can contain both scalar and message values.
    /// </summary>
    [Test]
    public void Parse_RepeatedFieldCanContainScalarAndMessage()
    {
        ProtoNode result = _parser.Parse("item: one item { id: 2 }");

        var values = (List<object?>)result["item"]!;

        Assert.Multiple(() =>
        {
            Assert.That(values[0], Is.EqualTo("one"));
            Assert.That(values[1], Is.TypeOf<ProtoNode>());
        });
    }

    /// <summary>
    /// Verifies that supported underscores, digits, and hyphens are accepted in field identifiers.
    /// </summary>
    [Test]
    public void Parse_AcceptsSupportedIdentifierCharacters()
    {
        ProtoNode result = _parser.Parse("_field2: value field-name_3: other");

        Assert.Multiple(() =>
        {
            Assert.That(result["_field2"], Is.EqualTo("value"));
            Assert.That(result["field-name_3"], Is.EqualTo("other"));
        });
    }

    /// <summary>
    /// Verifies that decimal values are parsed using the invariant culture.
    /// </summary>
    [Test]
    public void Parse_UsesInvariantCultureForDecimalValues()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-CA");

            ProtoNode result = _parser.Parse("value: 1234.5");

            Assert.That(result["value"], Is.EqualTo(1234.5d));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    /// <summary>
    /// Verifies that a parser instance resets its state between sequential parse operations.
    /// </summary>
    [Test]
    public void Parse_SameParserInstanceCanBeReusedSequentially()
    {
        ProtoNode first = _parser.Parse("value: 1");
        ProtoNode second = _parser.Parse("value: 2");

        Assert.Multiple(() =>
        {
            Assert.That(first["value"], Is.EqualTo(1L));
            Assert.That(second["value"], Is.EqualTo(2L));
        });
    }
}
