using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema;

internal readonly struct Token
{
    private readonly JsonElement _element;
    private readonly JsonPointer _elementPath;

    public Token(
        ref readonly JsonElement element,
        JsonPointer elementPath)
    {
        _element = element;
        _elementPath = elementPath;
    }

    public readonly JsonElement Element => _element;

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