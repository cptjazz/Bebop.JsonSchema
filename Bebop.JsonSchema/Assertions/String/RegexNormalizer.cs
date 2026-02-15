using System.Buffers;
using System.Text;

internal static class RegexNormalizer
{
    private static readonly SearchValues<char> BackslashP =
        SearchValues.Create("\\pPsS");

    // ECMA-262 \s includes all Unicode whitespace that .NET ECMAScript mode misses.
    // We expand \s to a character class covering the full spec.
    private const string Ecma262Whitespace =
        @"[\u0009\u000a\u000b\u000c\u000d\u0020\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]";
    private const string Ecma262NonWhitespace =
        @"[^\u0009\u000a\u000b\u000c\u000d\u0020\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]";

    private static readonly IReadOnlyDictionary<string, string> PropertyMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["digit"] = "Nd",
            ["decimalnumber"] = "Nd",
            ["letter"] = "L",
            ["uppercaseletter"] = "Lu",
            ["lowercaseletter"] = "Ll",
            ["titlecaseletter"] = "Lt",
            ["modifierletter"] = "Lm",
            ["otherletter"] = "Lo",
            ["number"] = "N",
            ["letternumber"] = "Nl",
            ["othernumber"] = "No",
            ["punctuation"] = "P",
            ["symbol"] = "S",
            ["separator"] = "Z",
            ["control"] = "Cc",
            ["format"] = "Cf",
            ["privateuse"] = "Co",
            ["surrogate"] = "Cs",
            ["unassigned"] = "Cn",
        };

    public static string Normalize(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return pattern;

        ReadOnlySpan<char> span = pattern;
        var sb = new StringBuilder(pattern.Length);

        int i = 0;

        while (i < span.Length)
        {
            int slashIndex = span[i..].IndexOfAny(BackslashP);
            if (slashIndex < 0)
            {
                sb.Append(span[i..]);
                break;
            }

            slashIndex += i;

            // Copy everything before candidate
            sb.Append(span[i..slashIndex]);

            if (span[slashIndex] != '\\')
            {
                sb.Append(span[slashIndex]);
                i = slashIndex + 1;
                continue;
            }

            // Handle \s and \S — expand to full ECMA-262 whitespace character class
            if (slashIndex + 1 < span.Length && span[slashIndex + 1] is 's' or 'S')
            {
                sb.Append(span[slashIndex + 1] == 's' ? Ecma262Whitespace : Ecma262NonWhitespace);
                i = slashIndex + 2;
                continue;
            }

            if (slashIndex + 2 >= span.Length ||
                (span[slashIndex + 1] != 'p' && span[slashIndex + 1] != 'P') ||
                span[slashIndex + 2] != '{')
            {
                sb.Append(span[slashIndex]);
                i = slashIndex + 1;
                continue;
            }

            bool isNegated = span[slashIndex + 1] == 'P';
            int start = slashIndex + 3;
            int end = span[start..].IndexOf('}');

            if (end < 0)
            {
                // malformed — copy literally
                sb.Append(span[slashIndex]);
                i = slashIndex + 1;
                continue;
            }

            end += start;

            ReadOnlySpan<char> propertyName = span[start..end];

            if (PropertyMap.TryGetValue(propertyName.ToString(), out var mapped))
            {
                sb.Append('\\');
                sb.Append(isNegated ? 'P' : 'p');
                sb.Append('{');
                sb.Append(mapped);
                sb.Append('}');
            }
            else
            {
                // leave untouched if not mapped
                sb.Append(span[slashIndex..(end + 1)]);
            }

            i = end + 1;
        }

        return sb.ToString();
    }
}
