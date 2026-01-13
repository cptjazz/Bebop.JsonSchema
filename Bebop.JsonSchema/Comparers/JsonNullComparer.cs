namespace Bebop.JsonSchema.Comparers;

internal sealed class JsonNullComparer : IJsonValueComparer
{
    public static readonly JsonNullComparer Instance = new();

    public bool AreEqual(in JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Null;
    }
}