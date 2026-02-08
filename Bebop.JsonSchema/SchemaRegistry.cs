using System.Globalization;

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
        string relative = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);

        return new Uri($"schema:anonymous/{relative}");
    }

    internal virtual int? EstimateSize() => null;
}