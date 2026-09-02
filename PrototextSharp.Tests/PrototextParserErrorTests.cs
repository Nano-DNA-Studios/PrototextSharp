namespace PrototextSharp.Tests;

public class PrototextParserErrorTests
{
    private readonly PrototextParser _parser = new();

    private static IEnumerable<TestCaseData> InvalidSyntaxCases()
    {
        yield return new TestCaseData(": 1", "Expected Identifier but found Colon").SetName("Missing field name");
        yield return new TestCaseData("name 1", "Expected Colon but found Number").SetName("Missing colon");
        yield return new TestCaseData("name:", "Expected value but found End").SetName("Missing value");
        yield return new TestCaseData("child { name: 1", "Expected CloseBrace but found End").SetName("Missing close brace");
        yield return new TestCaseData("}", "Expected End but found CloseBrace").SetName("Extra close brace");
        yield return new TestCaseData("name: { value: 1 }", "Expected value but found OpenBrace").SetName("Colon before nested message");
        yield return new TestCaseData("name: 1 extra", "Expected Colon but found End").SetName("Extra identifier after value");
        yield return new TestCaseData("name: true false", "Expected Identifier but found Boolean").SetName("Extra boolean after value");
        yield return new TestCaseData("name: 1e3", "Expected Colon but found End").SetName("Unsupported exponent notation");
    }

    /// <summary>
    /// Verifies that invalid grammar produces a format error describing the expected and actual token types.
    /// </summary>
    /// <param name="input">The invalid prototext to parse.</param>
    /// <param name="expectedMessage">The expected error message without its final period.</param>
    [TestCaseSource(nameof(InvalidSyntaxCases))]
    public void Parse_InvalidGrammar_ThrowsDescriptiveFormatException(string input, string expectedMessage)
    {
        FormatException? exception = Assert.Throws<FormatException>(() => _parser.Parse(input));

        Assert.That(exception!.Message, Is.EqualTo(expectedMessage + "."));
    }

    /// <summary>
    /// Verifies that unsupported characters produce a format error identifying the character and its position.
    /// </summary>
    /// <param name="input">The prototext containing an unsupported character.</param>
    /// <param name="character">The unsupported character expected in the error message.</param>
    [TestCase("name: @", "@")]
    [TestCase("name: +1", "+")]
    [TestCase("name: [1]", "[")]
    [TestCase("name: ;", ";")]
    [TestCase("name: .5", ".")]
    [TestCase("name: 1.2.3", ".")]
    public void Parse_UnexpectedCharacter_ThrowsFormatException(string input, string character)
    {
        FormatException? exception = Assert.Throws<FormatException>(() => _parser.Parse(input));

        Assert.That(exception!.Message, Does.Contain($"Unexpected character '{character}' at position"));
    }

    /// <summary>
    /// Verifies that a minus sign without any digits cannot be parsed as a number.
    /// </summary>
    [Test]
    public void Parse_LoneMinusAsValue_ThrowsFormatException()
    {
        Assert.That(() => _parser.Parse("name: -"), Throws.TypeOf<FormatException>());
    }

    /// <summary>
    /// Verifies that an integer outside the range of <see cref="long"/> produces an overflow error.
    /// </summary>
    [Test]
    public void Parse_IntegerOutsideInt64Range_ThrowsOverflowException()
    {
        Assert.That(
            () => _parser.Parse("name: 9223372036854775808"),
            Throws.TypeOf<OverflowException>());
    }

    /// <summary>
    /// Verifies that a decimal outside the range of <see cref="double"/> is represented as positive infinity.
    /// </summary>
    [Test]
    public void Parse_DecimalOutsideDoubleRange_ReturnsInfinity()
    {
        string input = "name: " + new string('9', 400) + ".0";

        ProtoNode result = _parser.Parse(input);

        Assert.That(result["name"], Is.EqualTo(double.PositiveInfinity));
    }

    /// <summary>
    /// Documents the parser's current null-input behavior as a null-reference error.
    /// </summary>
    [Test]
    public void Parse_NullInput_CharacterizesCurrentNullReferenceException()
    {
        Assert.That(() => _parser.Parse(null!), Throws.TypeOf<NullReferenceException>());
    }
}
