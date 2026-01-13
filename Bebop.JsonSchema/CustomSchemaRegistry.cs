using System.Diagnostics.CodeAnalysis;

namespace Bebop.JsonSchema;

internal sealed class CustomSchemaRegistry(ISchemaResolver resolver) : SchemaRegistry
{
    private readonly SchemaRegistry _baseRegistry = Local();

    public override void AddSchema(JsonSchema schema)
    {
        _baseRegistry.AddSchema(schema);
    }

    public override bool TryGetSchema(Uri id, [NotNullWhen(true)] out JsonSchema? schema)
    {
        if (_baseRegistry.TryGetSchema(id, out schema))
            return true;

        var resolveId = id.WithoutFragment();

        var json = resolver.Resolve(resolveId);
        if (!json.HasValue)
        {
            schema = null;
            return false;
        }

        // Schema must be available under its retrieval ID
        schema = JsonSchema.Create(json.Value, this, id, true);
        
        return true;
    }

    internal override int? EstimateSize() => _baseRegistry.EstimateSize();
}