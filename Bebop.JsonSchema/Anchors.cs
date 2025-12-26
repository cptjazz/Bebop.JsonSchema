using System.Diagnostics.CodeAnalysis;

namespace Bebop.JsonSchema;

internal sealed class Anchors
{
    private readonly Dictionary<string, JsonSchema> _anonymousAnchors = new();
    private readonly Dictionary<string, JsonSchema> _namedAnchors = new();
    private readonly Dictionary<string, JsonSchema> _dynamicNamedAnchors = new();

    public void AddAnonymousAnchor(in JsonPointer id, JsonSchema schema)
    {
        _anonymousAnchors[id.ToStringWithoutEncoding()] = schema;
    }

    public bool TryGetSchemaByAnonymousAnchor(in JsonPointer id, [NotNullWhen(true)] out JsonSchema? schema)
    {
        return _anonymousAnchors.TryGetValue(id.ToStringWithoutEncoding(), out schema);
    }

    public void AddNamedAnchor(in JsonPointer id, JsonSchema schema)
    {
        _namedAnchors[id.ToStringWithoutEncoding()] = schema;
    }

    public bool TryGetSchemaByNamedAnchor(in JsonPointer id, [NotNullWhen(true)] out JsonSchema? schema)
    {
        return _namedAnchors.TryGetValue(id.ToStringWithoutEncoding(), out schema);
    }

    public void AddDynamicNamedAnchor(in JsonPointer id, JsonSchema schema)
    {
        _dynamicNamedAnchors[id.ToStringWithoutEncoding()] = schema;
    }

    public bool TryGetSchemaDynamicNamedAnchor(in JsonPointer id, [NotNullWhen(true)] out JsonSchema? schema)
    {
        return _dynamicNamedAnchors.TryGetValue(id.ToStringWithoutEncoding(), out schema);
    }
}