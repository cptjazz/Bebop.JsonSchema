namespace Bebop.JsonSchema;

internal static class UriHelpers
{
    public static Uri WithoutFragment(this Uri uri)
    {
        if (uri is null) throw new ArgumentNullException(nameof(uri));

        if (uri.IsAbsoluteUri)
            return new UriBuilder(uri) { Fragment = string.Empty }.Uri;

        // relative URI
        var s = uri.OriginalString;
        var i = s.IndexOf('#');
        return i >= 0 ? new Uri(s[..i], UriKind.Relative) : uri;
    }

    public static bool GetFragment(this Uri uri, out ReadOnlySpan<char> result)
    {
        if (uri is null) 
            throw new ArgumentNullException(nameof(uri));

        var s = uri.OriginalString;
        var i = s.IndexOf('#');

        if (i >= 0)
        {
            result =  s.AsSpan()[i..];
            return true;
        }

        result = default;
        return false;
    }
}