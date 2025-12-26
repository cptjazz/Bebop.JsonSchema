namespace Bebop.JsonSchema;

internal sealed class LocalSchemaRegistry : SchemaRegistry
{
    private readonly Dictionary<string, JsonSchema> _schemas = new();

    public override void AddSchema(JsonSchema schema)
    {
        _schemas[schema.Id.AbsoluteUri] = schema;
    }

    public override ValueTask<JsonSchema?> GetSchema(Uri id)
    {
        return _schemas.TryGetValue(id.AbsoluteUri, out var schema) 
            ? ValueTask.FromResult<JsonSchema?>(schema)
            : ValueTask.FromResult<JsonSchema?>(null);
    }

    internal override int? EstimateSize() => _schemas.Count;
}