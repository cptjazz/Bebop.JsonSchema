using System.Collections.Concurrent;

namespace Tests.Bebop.JsonSchema.Infrastructure;

public sealed class CachingSchemaResolver : ISchemaResolver
{
    private readonly ConcurrentDictionary<string, JsonElement> _cache = new();
    private readonly HttpClient _httpClient;

    public CachingSchemaResolver()
    {
        _httpClient = new HttpClient();
    }

    public async ValueTask<JsonElement?> Resolve(Uri id)
    {
        if (id.Host.EndsWith("schemas.json"))
            return null;

        if (id.Host == "localhost")
        {
            return ResolveLocalFile(id);
        }

        return await ResolveRemote(id);
    }

    private JsonElement? ResolveLocalFile(Uri id)
    {
        var file = id.PathAndQuery;
        var path = Path.Join(AppContext.BaseDirectory, "TestSuite/remotes", file);

        if (!File.Exists(path))
            return null;

        using var stream = File.OpenRead(path);
        using var doc = JsonDocument.Parse(stream);
        return doc.RootElement.Clone();
    }

    private async Task<JsonElement?> ResolveRemote(Uri id)
    {
        var cacheKey = id.AbsoluteUri;

        if (_cache.TryGetValue(cacheKey, out var entry))
        {
            return entry;
        }

        var response = await _httpClient.GetAsync(id);

        if (!response.IsSuccessStatusCode)
            return null;

        using var doc = await JsonDocument.ParseAsync(response.Content.ReadAsStream());
        var schema = doc.RootElement.Clone();

        _cache.TryAdd(cacheKey, schema);

        return schema;
    }

    public void ClearCache()
    {
        _cache.Clear();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
