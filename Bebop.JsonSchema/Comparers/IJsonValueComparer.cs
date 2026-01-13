namespace Bebop.JsonSchema.Comparers;

internal interface IJsonValueComparer
{
    bool AreEqual(in JsonElement element);
}