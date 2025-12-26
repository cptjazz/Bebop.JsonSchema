namespace Bebop.JsonSchema.Comparers;

internal sealed class JsonObjectComparer(JsonElement constant) : IJsonValueComparer
{
    private readonly int _constantHashCode = JsonComparer.Instance.GetHashCode(constant);

    public bool AreEqual(in JsonElement element)
    {
        return _constantHashCode == JsonComparer.Instance.GetHashCode2(element) &&
               JsonComparer.Instance.Equals2(constant, element);
    }
}