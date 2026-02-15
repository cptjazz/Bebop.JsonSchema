namespace Bebop.JsonSchema;

public abstract class SchemaRegistry
{
    public static SchemaRegistry Local() => new LocalSchemaRegistry();

    public static SchemaRegistry Resolving() => new ResolvingSchemaRegistry(new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    });

    public static SchemaRegistry Custom(ISchemaResolver resolver) => new CustomSchemaRegistry(resolver);

    public static SchemaRegistry Resolving(HttpClient httpClient) => new ResolvingSchemaRegistry(httpClient);

    public abstract void AddSchema(JsonSchema schema);

    public abstract ValueTask<JsonSchema?> GetSchema(Uri id);

    internal Uri MakeRandomUri()
    {
        return new Uri(FallbackRetrievalUri, AlphaNum());
    }

    internal virtual int? EstimateSize() => null;

    internal virtual Uri FallbackRetrievalUri { get; } = new Uri($"schema:anonymous/{AlphaNum()}/");

    internal static string AlphaNum(int length = 12)
    {
        return string.Create(length, (object?)null, static (span, _) =>
        {
            ReadOnlySpan<char> chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = chars[Random.Shared.Next(chars.Length)];
            }
        });
    }
}
