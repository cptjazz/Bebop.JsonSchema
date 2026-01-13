using System.Diagnostics.CodeAnalysis;

namespace Bebop.JsonSchema;

internal sealed class LocalSchemaRegistry : SchemaRegistry
{
    private static readonly Dictionary<string, JsonSchema> _defaultSchemas = new();
    private readonly Dictionary<string, JsonSchema> _schemas = new();

    static LocalSchemaRegistry()
    {
        foreach (var schema in DefaultSchemas.Get())
        {
            _defaultSchemas[schema.Id.AbsoluteUri] = schema;
        }
    }

    public override void AddSchema(JsonSchema schema)
    {
        _schemas[schema.Id.AbsoluteUri] = schema;
    }

    public override bool TryGetSchema(Uri id, [NotNullWhen(true)] out JsonSchema? schema)
    {
        return _schemas.TryGetValue(id.AbsoluteUri, out schema) || 
               _defaultSchemas.TryGetValue(id.AbsoluteUri, out schema);
    }

    internal override int? EstimateSize() => _schemas.Count;
}