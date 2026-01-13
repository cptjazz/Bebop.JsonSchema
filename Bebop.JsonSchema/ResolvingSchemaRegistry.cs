using System.Diagnostics.CodeAnalysis;

namespace Bebop.JsonSchema;

internal sealed class ResolvingSchemaRegistry(HttpClient httpClient) : SchemaRegistry
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

        // Only resolve absolute http/https URIs -- discard urn or other schemes.
        if (!id.IsAbsoluteUri || (id.Scheme != Uri.UriSchemeHttp && id.Scheme != Uri.UriSchemeHttps))
        {
            schema = null;
            return false;
        }

        try
        {
            // TODO: Synchronous fetch (blocking). Consider async redesign if this becomes hot.
            using var stream = httpClient.GetStreamAsync(id).GetAwaiter().GetResult();
            using var doc = JsonDocument.Parse(stream);

            // Create will call AddSchema (via repository) so it gets cached.
            schema = JsonSchema.Create(doc, this);
            return true;
        }
        catch (Exception)
        {
            schema = null;
            return false;
        }
    }

    internal override int? EstimateSize() => _baseRegistry.EstimateSize();
}