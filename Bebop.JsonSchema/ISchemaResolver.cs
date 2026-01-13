namespace Bebop.JsonSchema;

public interface ISchemaResolver
{
    JsonElement? Resolve(Uri id);
}