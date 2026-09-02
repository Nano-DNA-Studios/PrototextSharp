namespace PrototextSharp.Tests;

public class PrototextStringAndCommentTests
{
    private readonly PrototextParser _parser = new();

    /// <summary>
    /// Verifies that an empty quoted value is parsed as an empty string.
    /// </summary>
    [Test]
    public void Parse_ParsesEmptyString()
    {
        ProtoNode result = _parser.Parse("value: \"\"");

        Assert.That(result["value"], Is.EqualTo(string.Empty));
    }

    /// <summary>
    /// Verifies that all supported escape sequences are decoded inside quoted strings.
    /// </summary>
    [Test]
    public void Parse_DecodesSupportedStringEscapes()
    {
        const string input = "value: \"quote=\\\" newline=\\n return=\\r tab=\\t slash=\\\\\"";

        ProtoNode result = _parser.Parse(input);

        Assert.That(result["value"], Is.EqualTo("quote=\" newline=\n return=\r tab=\t slash=\\"));
    }

    /// <summary>
    /// Verifies the current behavior for an unrecognized string escape sequence.
    /// </summary>
    [Test]
    public void Parse_UnrecognizedEscape_PreservesEscapedCharacterWithoutBackslash()
    {
        ProtoNode result = _parser.Parse("value: \"a\\qb\"");

        Assert.That(result["value"], Is.EqualTo("aqb"));
    }

    /// <summary>
    /// Verifies that a line break inside a quoted string is retained in the parsed value.
    /// </summary>
    [Test]
    public void Parse_AllowsLineBreakInsideString()
    {
        ProtoNode result = _parser.Parse("value: \"line1\nline2\"");

        Assert.That(result["value"], Is.EqualTo("line1\nline2"));
    }

    /// <summary>
    /// Verifies that input containing only a comment produces an empty root node.
    /// </summary>
    /// <param name="input">The hash-style or slash-style comment to parse.</param>
    [TestCase("# comment only")]
    [TestCase("// comment only")]
    [TestCase("# comment without final newline")]
    public void Parse_CommentOnlyInput_ReturnsEmptyNode(string input)
    {
        ProtoNode result = _parser.Parse(input);

        Assert.That(result.Fields, Is.Empty);
    }

    /// <summary>
    /// Verifies that hash-style and slash-style comments are skipped before and after fields.
    /// </summary>
    [Test]
    public void Parse_SkipsHashAndSlashCommentsAroundFields()
    {
        const string input = "# heading\r\nfirst: 1 # inline\r\n// second heading\nsecond: 2 // trailing without newline";

        ProtoNode result = _parser.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(result["first"], Is.EqualTo(1L));
            Assert.That(result["second"], Is.EqualTo(2L));
        });
    }

    /// <summary>
    /// Verifies that comment marker characters inside quoted strings are preserved as content.
    /// </summary>
    [Test]
    public void Parse_CommentMarkersInsideString_ArePreserved()
    {
        ProtoNode result = _parser.Parse("hash: \"value#fragment\" slash: \"https://example.test/path\"");

        Assert.Multiple(() =>
        {
            Assert.That(result["hash"], Is.EqualTo("value#fragment"));
            Assert.That(result["slash"], Is.EqualTo("https://example.test/path"));
        });
    }

    /// <summary>
    /// Verifies that a message whose lines are all commented out is not added to the parsed result.
    /// </summary>
    [Test]
    public void Parse_FullyCommentedMessage_IsIgnored()
    {
        const string input = "# item {\n#   id: 1\n# }\nitem { id: 2 }";

        ProtoNode result = _parser.Parse(input);
        var item = (ProtoNode)result["item"]!;

        Assert.That(item["id"], Is.EqualTo(2L));
    }

    /// <summary>
    /// Verifies that an unterminated quoted string produces a format error.
    /// </summary>
    [Test]
    public void Parse_UnterminatedString_ThrowsFormatException()
    {
        FormatException? exception = Assert.Throws<FormatException>(() => _parser.Parse("value: \"unfinished"));

        Assert.That(exception!.Message, Does.Contain("Unterminated string literal"));
    }

    /// <summary>
    /// Verifies that an unterminated quoted string ending in a backslash produces a format error.
    /// </summary>
    [Test]
    public void Parse_UnterminatedStringEndingInBackslash_ThrowsFormatException()
    {
        FormatException? exception = Assert.Throws<FormatException>(() => _parser.Parse("value: \"unfinished\\"));

        Assert.That(exception!.Message, Does.Contain("Unterminated string literal"));
    }

    /// <summary>
    /// Verifies that a slash which does not begin a comment produces an error containing its position.
    /// </summary>
    [Test]
    public void Parse_LoneSlash_ThrowsFormatExceptionWithPosition()
    {
        FormatException? exception = Assert.Throws<FormatException>(() => _parser.Parse("value: /"));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.Message, Does.Contain("Unexpected character '/'"));
            Assert.That(exception.Message, Does.Contain("position 7"));
        });
    }
}
