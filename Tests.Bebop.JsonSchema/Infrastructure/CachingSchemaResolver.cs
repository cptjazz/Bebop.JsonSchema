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

    public JsonElement? Resolve(Uri id)
    {
        if (id.Host.EndsWith("schemas.json"))
            return null;

        if (id.Host == "localhost")
        {
            return ResolveLocalFile(id);
        }

        return ResolveRemote(id);
    }

    private JsonElement? ResolveLocalFile(Uri id)
    {
        var file = id.PathAndQuery;
        var path = Path.Join(AppContext.BaseDirectory, "TestData/remotes", file);

        if (!File.Exists(path))
            return null;

        using var stream = File.OpenRead(path);
        using var doc = JsonDocument.Parse(stream);
        return doc.RootElement.Clone();
    }

    private JsonElement? ResolveRemote(Uri id)
    {
        var cacheKey = id.AbsoluteUri;

        if (_cache.TryGetValue(cacheKey, out var entry))
        {
            return entry;
        }

        var response = _httpClient.GetAsync(id).GetAwaiter().GetResult();

        if (!response.IsSuccessStatusCode)
            return null;

        var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        using var doc = JsonDocument.Parse(content);
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
