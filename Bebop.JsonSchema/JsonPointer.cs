using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Bebop.JsonSchema;

internal readonly struct JsonPointer : IEquatable<JsonPointer>
{
    private readonly string _pointer;
    private readonly int _hc;

    public static readonly JsonPointer Root = new("");
    
    internal JsonPointer(string pointer)
    {
        _pointer = pointer;
        _hc = pointer.GetHashCode();
    }

    private JsonPointer(string pointer, int hcHint)
    {
        _pointer = pointer;
        _hc = hcHint;
    }

    public static JsonPointer Parse(ReadOnlySpan<char> pointer)
    {
        return _Parse(pointer, false);
    }

    public static JsonPointer ParseFromUriFragment(ReadOnlySpan<char> pointer)
    {
        return _Parse(pointer, true);
    }

    private static JsonPointer _Parse(ReadOnlySpan<char> pointer, bool uriDecode)
    {
        if (pointer.IsEmpty)
        {
            return Root;
        }


        Span<char> target = stackalloc char[pointer.Length];

        var i = 0;
        var absPos = 0;

        while (true)
        {
            int segEnd = pointer.IndexOf('/');
            var unescaped = segEnd < 0 ? pointer : pointer[..segEnd];

            if (uriDecode)
            {
#if NET9_0_OR_GREATER
                unescaped = Uri.UnescapeDataString(unescaped);
#else
                unescaped = Uri.UnescapeDataString(unescaped.ToString());
#endif
            }


            var segEnd2 = _Decode(unescaped, target[absPos..]);
            absPos += segEnd2;

            if (segEnd < 0)
                break;

            target[absPos++] = '/';
            pointer = pointer[(unescaped.Length+1)..];

            if (pointer.IsEmpty)
                break;

            i++;
        }

        return new JsonPointer(new(target[..absPos]));
    }

    public override bool Equals(object? obj)
    {
        return obj is JsonPointer other && Equals(other);
    }

    public bool Equals(JsonPointer other)
    {
        return StringComparer.Ordinal.Equals(_pointer, other._pointer);
    }

    public override int GetHashCode()
    {
        return _hc;
    }

    public override string ToString() => ToStringWithoutEncoding();

    public string ToStringWithoutEncoding()
    {
        return _pointer;
    }

    private static int _Decode(ReadOnlySpan<char> segment, Span<char> target)
    {
        if (segment.IsEmpty)
            return 0;

        var absTargetIdx = 0;
        var tildeIdx = segment.IndexOf('~');
        
        while (tildeIdx >= 0)
        {
            segment[..tildeIdx].CopyTo(target[absTargetIdx..]);
            absTargetIdx += tildeIdx;

            segment = segment[(tildeIdx+1)..];
            if (segment.Length > 0)
            {
                if (segment[0] == '0')
                {
                    target[absTargetIdx++] = '~';
                    segment = segment[1..];
                }
                else if (segment[0] == '1')
                {
                    target[absTargetIdx++] = '/';
                    segment = segment[1..];
                }
            }

            tildeIdx = segment.IndexOf('~');
        }

        segment.CopyTo(target[absTargetIdx..]);
        return absTargetIdx + segment.Length;
    }

    internal JsonPointer AppendPropertyName(string segment)
    {
        return new JsonPointer(string.Concat(_pointer, "/", segment), _hc ^ segment.GetHashCode());
    }

    public JsonPointer AppendIndex(int idx)
    {
        Span<char> idxBuffer = stackalloc char[15]; // enough for int32 in invariant format
        return idx.TryFormat(idxBuffer, out var charsWritten, provider: CultureInfo.InvariantCulture) 
            ? new JsonPointer(string.Concat(_pointer, "/", idxBuffer[..charsWritten]), _hc ^ idx) 
            : _Throw();

        [DoesNotReturn]
        static JsonPointer _Throw()
        {
            throw new InvalidOperationException("Failed to format index.");
        }
    }
}