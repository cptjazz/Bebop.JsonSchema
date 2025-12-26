namespace Bebop.JsonSchema.Comparers;

internal sealed class JsonDoubleComparer(double constant) : IJsonValueComparer
{
    public bool AreEqual(in JsonElement element)
    {
        return element.TryGetDouble(out var d) && constant.Equals(d);
    }
}