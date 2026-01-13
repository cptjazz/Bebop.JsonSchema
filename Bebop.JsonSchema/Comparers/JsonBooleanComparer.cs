namespace Bebop.JsonSchema.Comparers;

internal sealed class JsonBooleanComparer(bool constant) : IJsonValueComparer
{
    public bool AreEqual(in JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.True)
        {
            return constant;
        }

        if (element.ValueKind == JsonValueKind.False)
        {
            return !constant;
        }

        return false;
    }
}