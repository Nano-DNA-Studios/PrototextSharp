namespace PrototextSharp.Tests;

/// <summary>
/// Verifies field storage and lookup behavior in <see cref="ProtoNode"/>.
/// </summary>
public class ProtoNodeTests
{
    /// <summary>
    /// Verifies that a newly constructed node contains no fields.
    /// </summary>
    [Test]
    public void Constructor_CreatesEmptyFieldsCollection()
    {
        var node = new ProtoNode();

        Assert.That(node.Fields, Is.Empty);
    }

    /// <summary>
    /// Verifies that requesting a field which does not exist returns null.
    /// </summary>
    [Test]
    public void Indexer_MissingKey_ReturnsNull()
    {
        var node = new ProtoNode();

        Assert.That(node["missing"], Is.Null);
    }

    /// <summary>
    /// Verifies that the first value for a field is returned directly by the indexer.
    /// </summary>
    [Test]
    public void Add_FirstValue_IndexerReturnsValueDirectly()
    {
        var node = new ProtoNode();

        node.Add("key", "value");

        Assert.Multiple(() =>
        {
            Assert.That(node["key"], Is.EqualTo("value"));
            Assert.That(node.Fields["key"], Has.Count.EqualTo(1));
        });
    }

    /// <summary>
    /// Verifies that repeated values are returned as a list in insertion order.
    /// </summary>
    [Test]
    public void Add_RepeatedValues_IndexerReturnsOrderedList()
    {
        var node = new ProtoNode();
        node.Add("key", 1L);
        node.Add("key", 2L);

        var values = (List<object?>)node["key"]!;

        Assert.That(values, Is.EqualTo(new object?[] { 1L, 2L }));
    }

    /// <summary>
    /// Verifies that a node can store both a child node and a null value.
    /// </summary>
    [Test]
    public void Add_CanStoreChildNodeAndNull()
    {
        var node = new ProtoNode();
        var child = new ProtoNode();
        node.Add("child", child);
        node.Add("nullable", null!);

        Assert.Multiple(() =>
        {
            Assert.That(node["child"], Is.SameAs(child));
            Assert.That(node.Fields["nullable"], Has.Count.EqualTo(1));
            Assert.That(node["nullable"], Is.Null);
        });
    }

    /// <summary>
    /// Verifies that field names differing only by letter casing remain distinct.
    /// </summary>
    [Test]
    public void Keys_AreCaseSensitive()
    {
        var node = new ProtoNode();
        node.Add("Name", "upper");
        node.Add("name", "lower");

        Assert.Multiple(() =>
        {
            Assert.That(node["Name"], Is.EqualTo("upper"));
            Assert.That(node["name"], Is.EqualTo("lower"));
        });
    }
}
