namespace Bebop.JsonSchema;

internal sealed class CustomSchemaRegistry(ISchemaResolver resolver) : SchemaRegistry
{
    private readonly SchemaRegistry _baseRegistry = Local();

    public override void AddSchema(JsonSchema schema)
    {
        _baseRegistry.AddSchema(schema);
    }

    public override async ValueTask<JsonSchema?> GetSchema(Uri id)
    {
        var schema = await _baseRegistry
            .GetSchema(id)
            .ConfigureAwait(false);

        if (schema != null)
            return schema;

        var resolveId = id.WithoutFragment();

        var json = await resolver
            .Resolve(resolveId)
            .ConfigureAwait(false);

        if (!json.HasValue)
        {
            return null;
        }

        // Schema must be available under its retrieval ID
        return await JsonSchema
            .Create(json.Value, this, id, true)
            .ConfigureAwait(false);
    }

    internal override int? EstimateSize() => _baseRegistry.EstimateSize();
}