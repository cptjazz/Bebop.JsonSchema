namespace Bebop.JsonSchema;

internal sealed class ResolvingSchemaRegistry(HttpClient httpClient) : SchemaRegistry
{
    private readonly SchemaRegistry _baseRegistry = Local();

    public override void AddSchema(JsonSchema schema)
    {
        _baseRegistry.AddSchema(schema);
    }

    public override async ValueTask<JsonSchema?> GetSchema(Uri id)
    {
        var schema = await _baseRegistry.GetSchema(id);
        if (schema is not null)
            return schema;

        // Only resolve absolute http/https URIs -- discard urn or other schemes.
        if (!id.IsAbsoluteUri || (id.Scheme != Uri.UriSchemeHttp && id.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        try
        {
            // TODO: Synchronous fetch (blocking). Consider async redesign if this becomes hot.
            using var stream = await httpClient
                .GetStreamAsync(id)
                .ConfigureAwait(false);

            using var doc = await JsonDocument
                .ParseAsync(stream)
                .ConfigureAwait(false);

            // Create will call AddSchema (via repository) so it gets cached.
            return await JsonSchema
                .Create(doc, this)
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal override int? EstimateSize() => _baseRegistry.EstimateSize();
}