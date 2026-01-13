namespace Bebop.JsonSchema.Comparers;

internal sealed class JsonStringComparer(string? constant) : IJsonValueComparer
{
    public bool AreEqual(in JsonElement element)
    {
        return element.ValueKind == JsonValueKind.String && StringComparer.Ordinal.Equals(constant, element.GetString());
    }
}