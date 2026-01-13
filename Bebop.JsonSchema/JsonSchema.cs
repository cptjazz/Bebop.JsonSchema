using System.Diagnostics;

namespace Bebop.JsonSchema;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class JsonSchema
{
    public static readonly JsonSchema True = _TrueSchema();

    public static readonly JsonSchema False = _FalseSchema();

    private static JsonSchema _TrueSchema()
    {
        return SchemaParser.ParseSchema(JsonSerializer.SerializeToElement(true), SchemaRegistry.Local(), null);
    }

    private static JsonSchema _FalseSchema()
    {
        return SchemaParser.ParseSchema(JsonSerializer.SerializeToElement(false), SchemaRegistry.Local(), null);
    }


    internal JsonSchema(Uri id, SchemaRegistry repository, Anchors anchors, bool isAnonymous)
    {
        Id = id;
        Repository = repository;
        Anchors = anchors;
        IsAnonymous = isAnonymous;
        RootAssertion = AnyTypeAssertion.Instance;
    }
    
    internal Assertion RootAssertion { get; set; }

    public Uri Id { get; }
    
    internal SchemaRegistry Repository { get; }

    internal Anchors Anchors { get; }

    internal bool IsAnonymous { get; }

    public string? Comment { get; internal set; }
    
    public string? Description { get; internal set; }

    internal JsonPointer Path { get; set; }

    internal string DebuggerDisplay => $"{Path}";

    public static JsonSchema Create(string json) => Create(JsonDocument.Parse(json), SchemaRegistry.Local());

    public static JsonSchema Create(JsonDocument document) => Create(document, SchemaRegistry.Local());

    public static JsonSchema Create(JsonDocument document, SchemaRegistry repository)
    {
        return Create(document.RootElement.Clone(), repository, null, true);
    }

    public static JsonSchema Create(JsonElement document, SchemaRegistry repository)
    {
        return Create(document, repository, null, true);
    }

    internal static JsonSchema Create(JsonElement element, SchemaRegistry repository, Uri? retrievalUri, bool addToRepo)
    {
        var me = SchemaParser.ParseSchema(element, repository, retrievalUri);
        
        if (addToRepo)
        {
            repository.AddSchema(me);
        }

        return me;
    }

    public bool Validate(JsonDocument document, ErrorCollection errorCollection)
    {
        return Validate(document.RootElement, errorCollection);
    }

    public bool Validate(in JsonElement element, ErrorCollection errorCollection)
    {
        var allocSize = Math.Min(50, Repository.EstimateSize() ?? 20);
        return Validate(new (in element, JsonPointer.Root), new(new(allocSize)), errorCollection);
    }

    internal bool Validate(Token token, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        evaluationState.PushSchema(this);
        var result = RootAssertion.Assert(token, evaluationState, errorCollection);
        evaluationState.PopSchema();

        return result;
    }

    public ValueTask Prepare()
    {
        return RootAssertion.Prepare();
    }
}