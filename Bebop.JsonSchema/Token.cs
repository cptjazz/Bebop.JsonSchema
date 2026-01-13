using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema;

internal readonly ref struct Token
{
    private readonly ref JsonElement _element;
    private readonly JsonPointer _elementPath;

    public Token(
        ref readonly JsonElement element,
        JsonPointer elementPath)
    {
        _element = ref Unsafe.AsRef(in element);
        _elementPath = elementPath;
    }

    public ref readonly JsonElement Element => ref _element;

    public JsonPointer ElementPath => _elementPath;

    public Token Subitem(ref readonly JsonElement el, string propertyName)
    {
        return new Token(in el, _elementPath.AppendPropertyName(propertyName));
    }

    public Token Subitem(ref readonly JsonElement el, int index)
    {
        return new Token(in el, _elementPath.AppendIndex(index));
    }
}