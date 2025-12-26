namespace Bebop.JsonSchema;

public interface ISchemaResolver
{
    ValueTask<JsonElement?> Resolve(Uri id);
}