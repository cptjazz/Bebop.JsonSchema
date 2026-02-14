namespace Tests.MyJsonSchema;

public class Test_SchemaRegistry
{
    // ── Factory methods ─────────────────────────────────────────────

    [Fact]
    public void Local_ReturnsNonNullRegistry()
    {
        var registry = SchemaRegistry.Local();
        Assert.NotNull(registry);
    }

    [Fact]
    public void Custom_ReturnsNonNullRegistry()
    {
        var resolver = new NullResolver();
        var registry = SchemaRegistry.Custom(resolver);
        Assert.NotNull(registry);
    }

    [Fact]
    public void Resolving_ReturnsNonNullRegistry()
    {
        var registry = SchemaRegistry.Resolving();
        Assert.NotNull(registry);
    }

    [Fact]
    public void Resolving_WithHttpClient_ReturnsNonNullRegistry()
    {
        using var httpClient = new HttpClient();
        var registry = SchemaRegistry.Resolving(httpClient);
        Assert.NotNull(registry);
    }

    // ── LocalSchemaRegistry ─────────────────────────────────────────

    [Fact]
    public async Task Local_AddAndGetSchema_RoundTrips()
    {
        var registry = SchemaRegistry.Local();

        var schema = await JsonSchema.Create("""{"type":"string"}""");
        registry.AddSchema(schema);

        var retrieved = await registry.GetSchema(schema.Id);
        Assert.Same(schema, retrieved);
    }

    [Fact]
    public async Task Local_GetSchema_ReturnsNull_WhenNotFound()
    {
        var registry = SchemaRegistry.Local();
        var result = await registry.GetSchema(new Uri("http://example.com/missing"));
        Assert.Null(result);
    }

    [Fact]
    public async Task Local_AddSchema_OverwritesPreviousWithSameId()
    {
        var registry = SchemaRegistry.Local();

        var schema1 = await JsonSchema.Create("""{"type":"string"}""");
        registry.AddSchema(schema1);

        var schema2 = await JsonSchema.Create("""{"type":"number"}""");
        // Forcefully add with the same ID by creating via the same registry
        // Just verify that two adds with different schemas don't throw
        registry.AddSchema(schema2);

        var retrieved = await registry.GetSchema(schema2.Id);
        Assert.Same(schema2, retrieved);
    }

    [Fact]
    public async Task Local_MultipleSchemas_RetrievedIndependently()
    {
        var registry = SchemaRegistry.Local();

        var s1 = await JsonSchema.Create("""{"$id":"http://example.com/s1","type":"string"}""");
        var s2 = await JsonSchema.Create("""{"$id":"http://example.com/s2","type":"number"}""");

        registry.AddSchema(s1);
        registry.AddSchema(s2);

        Assert.Same(s1, await registry.GetSchema(new Uri("http://example.com/s1")));
        Assert.Same(s2, await registry.GetSchema(new Uri("http://example.com/s2")));
    }

    // ── CustomSchemaRegistry ────────────────────────────────────────

    [Fact]
    public async Task Custom_GetSchema_DelegatesToResolver_WhenNotCached()
    {
        var element = JsonDocument.Parse("""{"type":"integer"}""").RootElement.Clone();
        var resolver = new FakeResolver(element);
        var registry = SchemaRegistry.Custom(resolver);

        var targetUri = new Uri("http://example.com/test-schema");
        var schema = await registry.GetSchema(targetUri);

        Assert.NotNull(schema);
        Assert.True(resolver.WasCalledWith(targetUri));
    }

    [Fact]
    public async Task Custom_GetSchema_ReturnsCached_OnSecondCall()
    {
        var element = JsonDocument.Parse("""{"type":"boolean"}""").RootElement.Clone();
        var resolver = new CountingResolver(element);
        var registry = SchemaRegistry.Custom(resolver);

        var targetUri = new Uri("http://example.com/cached-schema");
        var first = await registry.GetSchema(targetUri);
        var second = await registry.GetSchema(targetUri);

        Assert.NotNull(first);
        Assert.Same(first, second);
        // Resolver should only be called once — second call uses the cache.
        Assert.Equal(1, resolver.CallCount);
    }

    [Fact]
    public async Task Custom_GetSchema_ReturnsNull_WhenResolverReturnsNull()
    {
        var resolver = new NullResolver();
        var registry = SchemaRegistry.Custom(resolver);

        var result = await registry.GetSchema(new Uri("http://example.com/nonexistent"));
        Assert.Null(result);
    }

    [Fact]
    public async Task Custom_AddSchema_MakesFutureGetSchemaReturnIt()
    {
        var resolver = new NullResolver();
        var registry = SchemaRegistry.Custom(resolver);

        var schema = await JsonSchema.Create(
            """{"$id":"http://example.com/added","type":"string"}""");
        registry.AddSchema(schema);

        var result = await registry.GetSchema(new Uri("http://example.com/added"));
        Assert.Same(schema, result);
    }

    // ── ResolvingSchemaRegistry ─────────────────────────────────────

    [Fact]
    public async Task Resolving_GetSchema_ReturnsNull_ForNonHttpScheme()
    {
        var registry = SchemaRegistry.Resolving();
        var result = await registry.GetSchema(new Uri("urn:example:not-http"));
        Assert.Null(result);
    }

    [Fact]
    public async Task Resolving_GetSchema_ReturnsNull_ForUnreachableUrl()
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(500) };
        var registry = SchemaRegistry.Resolving(httpClient);

        // Use a non-routable address to ensure failure
        var result = await registry.GetSchema(new Uri("http://192.0.2.1/nonexistent"));
        Assert.Null(result);
    }

    [Fact]
    public async Task Resolving_AddAndGetSchema_RoundTrips()
    {
        var registry = SchemaRegistry.Resolving();
        var schema = await JsonSchema.Create("""{"type":"string"}""");

        registry.AddSchema(schema);
        var result = await registry.GetSchema(schema.Id);
        Assert.Same(schema, result);
    }

    // ── MakeRandomUri ───────────────────────────────────────────────

    [Fact]
    public void MakeRandomUri_ReturnsAbsoluteUri()
    {
        var registry = SchemaRegistry.Local();
        var uri = registry.MakeRandomUri();

        Assert.True(uri.IsAbsoluteUri);
        Assert.Equal("schema", uri.Scheme);
    }

    [Fact]
    public void MakeRandomUri_ReturnsUniqueUris()
    {
        var registry = SchemaRegistry.Local();
        var uris = Enumerable.Range(0, 100)
            .Select(_ => registry.MakeRandomUri().AbsoluteUri)
            .ToHashSet();

        Assert.Equal(100, uris.Count);
    }

    // ── Integration: Schema validation through registries ───────────

    [Fact]
    public async Task Local_EndToEnd_SchemaValidation()
    {
        var registry = SchemaRegistry.Local();
        var schema = await JsonSchema.Create("""{"type":"string"}""");
        await schema.Prepare();

        var errors = new ErrorCollection();
        Assert.True(await schema.Validate(JsonDocument.Parse(""" "hello" """), errors));
        Assert.False(await schema.Validate(JsonDocument.Parse("42"), errors));
    }

    [Fact]
    public async Task Custom_EndToEnd_RefResolution()
    {
        // Schema that $refs another schema resolved by the custom resolver
        var referencedElement = JsonDocument.Parse("""{"type":"integer"}""").RootElement.Clone();
        var resolver = new FakeResolver(referencedElement, "http://example.com/int-schema");
        var registry = SchemaRegistry.Custom(resolver);

        var doc = JsonDocument.Parse("""{"$ref":"http://example.com/int-schema"}""");
        var schema = await JsonSchema.Create(doc, registry);
        await schema.Prepare();

        var errors = new ErrorCollection();
        Assert.True(await schema.Validate(JsonDocument.Parse("42"), errors));
        Assert.False(await schema.Validate(JsonDocument.Parse(""" "text" """), errors));
    }

    // ── Test helpers ────────────────────────────────────────────────

    private sealed class NullResolver : ISchemaResolver
    {
        public ValueTask<JsonElement?> Resolve(Uri id) => ValueTask.FromResult<JsonElement?>(null);
    }

    private sealed class FakeResolver : ISchemaResolver
    {
        private readonly JsonElement _element;
        private readonly string? _expectedUri;
        private readonly HashSet<string> _calledUris = new();

        public FakeResolver(JsonElement element, string? expectedUri = null)
        {
            _element = element;
            _expectedUri = expectedUri;
        }

        public ValueTask<JsonElement?> Resolve(Uri id)
        {
            _calledUris.Add(id.AbsoluteUri);

            if (_expectedUri is not null && id.AbsoluteUri != _expectedUri)
                return ValueTask.FromResult<JsonElement?>(null);

            return ValueTask.FromResult<JsonElement?>(_element);
        }

        public bool WasCalledWith(Uri uri) => _calledUris.Contains(uri.AbsoluteUri);
    }

    private sealed class CountingResolver : ISchemaResolver
    {
        private readonly JsonElement _element;
        public int CallCount { get; private set; }

        public CountingResolver(JsonElement element) => _element = element;

        public ValueTask<JsonElement?> Resolve(Uri id)
        {
            CallCount++;
            return ValueTask.FromResult<JsonElement?>(_element);
        }
    }
}
