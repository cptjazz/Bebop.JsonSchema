namespace Tests.MyJsonSchema;

public class Test_UriHelpers
{
    // ── WithoutFragment ─────────────────────────────────────────────

    [Fact]
    public void WithoutFragment_RemovesFragmentFromAbsoluteUri()
    {
        var uri = new Uri("http://example.com/path#fragment");

        var result = uri.WithoutFragment();

        Assert.Equal("http://example.com/path", result.AbsoluteUri);
    }

    [Fact]
    public void WithoutFragment_ReturnsAbsoluteUriUnchangedWhenNoFragment()
    {
        var uri = new Uri("http://example.com/path");

        var result = uri.WithoutFragment();

        Assert.Equal("http://example.com/path", result.AbsoluteUri);
    }

    [Fact]
    public void WithoutFragment_RemovesFragmentFromRelativeUri()
    {
        var uri = new Uri("path/to/resource#fragment", UriKind.Relative);

        var result = uri.WithoutFragment();

        Assert.Equal("path/to/resource", result.OriginalString);
    }

    [Fact]
    public void WithoutFragment_ReturnsRelativeUriUnchangedWhenNoFragment()
    {
        var uri = new Uri("path/to/resource", UriKind.Relative);

        var result = uri.WithoutFragment();

        Assert.Same(uri, result);
    }

    // ── GetFragment ─────────────────────────────────────────────────

    [Fact]
    public void GetFragment_ReturnsTrueAndFragmentWhenPresent()
    {
        var uri = new Uri("path#my-fragment", UriKind.Relative);

        var result = uri.GetFragment(out var fragment);

        Assert.True(result);
        Assert.Equal("#my-fragment", fragment.ToString());
    }

    [Fact]
    public void GetFragment_ReturnsFalseWhenNoFragment()
    {
        var uri = new Uri("path/no-fragment", UriKind.Relative);

        var result = uri.GetFragment(out var fragment);

        Assert.False(result);
        Assert.True(fragment.IsEmpty);
    }
}
