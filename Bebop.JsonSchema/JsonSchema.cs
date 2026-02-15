using System.Diagnostics;

namespace Bebop.JsonSchema;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class JsonSchema
{
    public static readonly JsonSchema True = _TrueSchema();

    public static readonly JsonSchema False = _FalseSchema();

    private static JsonSchema _TrueSchema()
    {
        var repo = SchemaRegistry.Local();
        var js = new JsonSchema(repo.MakeRandomUri(), repo, new(), false, Dialect.Get(Dialect.DefaultDialectUri)!);

        js.RootAssertion = AnyTypeAssertion.Instance;
        js.IsPrepared = true;

        return js;
    }

    private static JsonSchema _FalseSchema()
    {
        var repo = SchemaRegistry.Local();
        var js = new JsonSchema(repo.MakeRandomUri(), repo, new(), false, Dialect.Get(Dialect.DefaultDialectUri)!);

        js.RootAssertion = NoneTypeAssertion.Instance;
        js.IsPrepared = true;

        return js;
    }


    internal JsonSchema(Uri id, SchemaRegistry repository, Anchors anchors, bool isAnonymous, Dialect dialect)
    {
        Id = id;
        Repository = repository;
        Anchors = anchors;
        IsAnonymous = isAnonymous;
        RootAssertion = AnyTypeAssertion.Instance;
        Dialect = dialect;
    }
    
    internal Assertion RootAssertion { get; set; }

    public Uri Id { get; }
    
    internal SchemaRegistry Repository { get; }

    internal Anchors Anchors { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal bool IsAnonymous { get; }

    public string? Comment { get; internal set; }
    
    public string? Description { get; internal set; }

    internal JsonPointer Path { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal IReadOnlySet<Uri>? Vocabulary { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal Dialect Dialect { get; }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal string DebuggerDisplay => $"{Path}";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal bool IsPrepared { get; private set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly SemaphoreSlim _preparationSemaphore = new(1, 1);
    
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool _isPreparing;

    public static ValueTask<JsonSchema> Create(string json) => Create(JsonDocument.Parse(json), SchemaRegistry.Local());

    public static ValueTask<JsonSchema> Create(JsonDocument document) => Create(document, SchemaRegistry.Local());

    public static ValueTask<JsonSchema> Create(JsonDocument document, SchemaRegistry repository)
    {
        return Create(document.RootElement.Clone(), repository, repository.FallbackRetrievalUri, true);
    }

    public static ValueTask<JsonSchema> Create(JsonElement document, SchemaRegistry repository)
    {
        return Create(document, repository, repository.FallbackRetrievalUri, true);
    }

    public static ValueTask<JsonSchema> Create(JsonDocument document, Uri retrievalUri, SchemaRegistry repository)
    {
        return Create(document.RootElement.Clone(), repository, retrievalUri, true);
    }

    public static ValueTask<JsonSchema> Create(JsonElement document, Uri retrievalUri, SchemaRegistry repository)
    {
        return Create(document, repository, retrievalUri, true);
    }

    internal static async ValueTask<JsonSchema> Create(JsonElement element, SchemaRegistry repository, Uri? retrievalUri, bool addToRepo)
    {
        var me = await SchemaParser
            .ParseSchema(element, repository, retrievalUri)
            .ConfigureAwait(false);
        
        if (addToRepo)
        {
            repository.AddSchema(me);
        }

        return me;
    }

    public ValueTask<bool> Validate(JsonDocument document, ErrorCollection errorCollection)
    {
        return Validate(document.RootElement, errorCollection);
    }

    public ValueTask<bool> Validate(in JsonElement element, ErrorCollection errorCollection)
    {
        var allocSize = Math.Min(50, Repository.EstimateSize() ?? 20);
        return Validate(new (in element, JsonPointer.Root), new(new(allocSize)), errorCollection);
    }

    internal async ValueTask<bool> Validate(Token token, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        _EnsurePrepared();

        evaluationState.PushSchema(this);
        var result = await RootAssertion.Assert(token, evaluationState, errorCollection).ConfigureAwait(false);
        evaluationState.PopSchema();

        return result;
    }

    private void _EnsurePrepared()
    {
        if (!IsPrepared)
        {
            throw new InvalidOperationException("Schema must be prepared before validation. Call Prepare() first.");
        }
    }

    public async ValueTask Prepare()
    {
        if (Volatile.Read(ref _isPreparing))
            return;

        await SyncContext.Drop();
        await _preparationSemaphore.WaitAsync();
        
        try
        {
            if (IsPrepared)
                return;

            Volatile.Write(ref _isPreparing, true);
            await RootAssertion.Prepare();
            Volatile.Write(ref _isPreparing, false);

            IsPrepared = true;
        }
        finally
        {
            _preparationSemaphore.Release();
        }        
    }
}
