namespace Bebop.JsonSchema.Comparers;

internal sealed class JsonComparer : IEqualityComparer<JsonElement>
{
    public static readonly JsonComparer Instance = new();

    private static bool _AreObjectsEqual(in JsonElement obj1, in JsonElement obj2)
    {
        if (obj1.GetPropertyCount() != obj2.GetPropertyCount())
        {
            return false;
        }

        foreach (var kvp in obj1.EnumerateObject())
        {
            if (!obj2.TryGetProperty(kvp.Name, out var value2) || !Instance.Equals(kvp.Value, value2))
            {
                return false;
            }
        }

        return true;
    }

    private static bool _AreArraysEqual(in JsonElement arr1, in JsonElement arr2)
    {
        return arr1
            .EnumerateArray()
            .SequenceEqual(
                arr2.EnumerateArray(), 
                Instance);
    }

    public bool Equals(JsonElement x, JsonElement y)
    {
        return Equals2(in x, in y);
    }

    public bool Equals2(in JsonElement x, in JsonElement y)
    {
        if (x.ValueKind != y.ValueKind)
            return false;

        return x.ValueKind switch
        {
            JsonValueKind.Object => _AreObjectsEqual(x, y),
            JsonValueKind.Array => _AreArraysEqual(x, y),
            JsonValueKind.String => x.GetString() == y.GetString(),
            JsonValueKind.Number => x.GetDouble() == y.GetDouble(),
            JsonValueKind.True or JsonValueKind.False => x.GetBoolean() == y.GetBoolean(),
            JsonValueKind.Null => true,
            _ => throw new NotImplementedException()
        };
    }

    public int GetHashCode(JsonElement obj)
    {
        return GetHashCode2(obj);
    }

    public int GetHashCode2(in JsonElement obj)
    {
        return (int)obj.ValueKind;
    }
}