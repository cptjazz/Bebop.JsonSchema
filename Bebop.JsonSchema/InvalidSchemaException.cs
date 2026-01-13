namespace Bebop.JsonSchema;

public sealed class InvalidSchemaException : Exception
{
    public InvalidSchemaException(string error)
        : base(error)
    {
    }

    public InvalidSchemaException(string error, JsonProperty property) 
        : this($"{error} (Property: '{property.Name}')")
    {
    }

    public InvalidSchemaException(string error, JsonElement element)
        : this($"{error} (Element: {element})")
    {
    }
}