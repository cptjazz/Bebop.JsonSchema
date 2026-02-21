namespace Tests.MyJsonSchema;

public class Test_JsonSchema
{
    // ── Create overloads ────────────────────────────────────────────

    [Fact]
    public async Task Create_FromString_ReturnsSchema()
    {
        var schema = await JsonSchema.Create("""{"type":"string"}""");

        Assert.NotNull(schema);
    }

    [Fact]
    public async Task Create_FromJsonDocument_ReturnsSchema()
    {
        using var doc = JsonDocument.Parse("""{"type":"integer"}""");

        var schema = await JsonSchema.Create(doc);

        Assert.NotNull(schema);
    }

    [Fact]
    public async Task Create_FromJsonDocumentWithRegistry_ReturnsSchema()
    {
        using var doc = JsonDocument.Parse("""{"type":"number"}""");
        var registry = SchemaRegistry.Local();

        var schema = await JsonSchema.Create(doc, registry);

        Assert.NotNull(schema);
    }

    [Fact]
    public async Task Create_FromJsonElementWithRegistry_ReturnsSchema()
    {
        using var doc = JsonDocument.Parse("""{"type":"boolean"}""");
        var registry = SchemaRegistry.Local();

        var schema = await JsonSchema.Create(doc.RootElement, registry);

        Assert.NotNull(schema);
    }

    [Fact]
    public async Task Create_FromJsonDocumentWithRetrievalUriAndRegistry_ReturnsSchema()
    {
        using var doc = JsonDocument.Parse("""{"type":"string"}""");
        var registry = SchemaRegistry.Local();
        var retrievalUri = new Uri("http://example.com/schema.json");

        var schema = await JsonSchema.Create(doc, retrievalUri, registry);

        Assert.NotNull(schema);
    }

    [Fact]
    public async Task Create_FromJsonElementWithRetrievalUriAndRegistry_ReturnsSchema()
    {
        using var doc = JsonDocument.Parse("""{"type":"string"}""");
        var registry = SchemaRegistry.Local();
        var retrievalUri = new Uri("http://example.com/schema.json");

        var schema = await JsonSchema.Create(doc.RootElement, retrievalUri, registry);

        Assert.NotNull(schema);
    }

    // ── Validate before Prepare ─────────────────────────────────────

    [Fact]
    public async Task Validate_WithoutPrepare_ThrowsInvalidOperationException()
    {
        var schema = await JsonSchema.Create("""{"type":"string"}""");

        using var doc = JsonDocument.Parse(""" "hello" """);
        var errors = new ErrorCollection();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => schema.Validate(doc, errors).AsTask());
    }

    [Fact]
    public async Task Validate_WithoutPrepare_ThrowsForJsonElementOverload()
    {
        var schema = await JsonSchema.Create("""{"type":"string"}""");

        using var doc = JsonDocument.Parse(""" "hello" """);
        var errors = new ErrorCollection();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => schema.Validate(doc.RootElement, errors).AsTask());
    }

    // ── Prepare ─────────────────────────────────────────────────────

    [Fact]
    public async Task Prepare_CalledTwice_DoesNotThrow()
    {
        var schema = await JsonSchema.Create("""{"type":"string"}""");

        await schema.Prepare();
        await schema.Prepare();
    }

    // ── True / False singletons ─────────────────────────────────────

    [Fact]
    public async Task TrueSchema_AlwaysValidates()
    {
        var errors = new ErrorCollection();

        Assert.True(await JsonSchema.True.Validate(JsonDocument.Parse("42"), errors));
        Assert.True(await JsonSchema.True.Validate(JsonDocument.Parse(""" "text" """), errors));
        Assert.True(await JsonSchema.True.Validate(JsonDocument.Parse("null"), errors));
    }

    [Fact]
    public async Task FalseSchema_NeverValidates()
    {
        var errors = new ErrorCollection();

        Assert.False(await JsonSchema.False.Validate(JsonDocument.Parse("42"), errors));
        Assert.False(await JsonSchema.False.Validate(JsonDocument.Parse(""" "text" """), errors));
        Assert.False(await JsonSchema.False.Validate(JsonDocument.Parse("null"), errors));
    }

    // ── Metadata ────────────────────────────────────────────────────

    [Fact]
    public async Task Create_PreservesComment()
    {
        var schema = await JsonSchema.Create("""{"$comment":"test comment","type":"string"}""");

        Assert.Equal("test comment", schema.Comment);
    }

    [Fact]
    public async Task Create_PreservesDescription()
    {
        var schema = await JsonSchema.Create("""{"description":"A test description","type":"string"}""");

        Assert.Equal("A test description", schema.Description);
    }
}
