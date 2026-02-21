namespace Tests.MyJsonSchema;

public class Test_Dialect_Draft202012
{
    private readonly Dialect_Draft202012 _dialect = Dialect_Draft202012.Instance;

    // ── GetKeywordSet ───────────────────────────────────────────────

    [Theory]
    [InlineData(Vocabularies_Draft202012.Core)]
    [InlineData(Vocabularies_Draft202012.Applicator)]
    [InlineData(Vocabularies_Draft202012.Validation)]
    [InlineData(Vocabularies_Draft202012.Metadata)]
    [InlineData(Vocabularies_Draft202012.FormatAnnotation)]
    [InlineData(Vocabularies_Draft202012.FormatAssertion)]
    [InlineData(Vocabularies_Draft202012.Content)]
    [InlineData(Vocabularies_Draft202012.Unevaluated)]
    public void GetKeywordSet_ReturnsNonEmptySetForKnownVocabulary(string vocab)
    {
        var result = _dialect.GetKeywordSet(new Uri(vocab));

        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetKeywordSet_ThrowsForUnknownVocabulary()
    {
        var unknown = new Uri("https://example.com/vocab/unknown");

        Assert.Throws<InvalidOperationException>(() => _dialect.GetKeywordSet(unknown));
    }

    [Fact]
    public void GetKeywordSet_FormatAnnotationAndAssertionReturnSameSet()
    {
        var annotation = _dialect.GetKeywordSet(new Uri(Vocabularies_Draft202012.FormatAnnotation));
        var assertion = _dialect.GetKeywordSet(new Uri(Vocabularies_Draft202012.FormatAssertion));

        Assert.Same(annotation, assertion);
    }

    // ── TryGetKeywordSet ────────────────────────────────────────────

    [Theory]
    [InlineData(Vocabularies_Draft202012.Core)]
    [InlineData(Vocabularies_Draft202012.Applicator)]
    [InlineData(Vocabularies_Draft202012.Validation)]
    [InlineData(Vocabularies_Draft202012.Metadata)]
    [InlineData(Vocabularies_Draft202012.FormatAnnotation)]
    [InlineData(Vocabularies_Draft202012.FormatAssertion)]
    [InlineData(Vocabularies_Draft202012.Content)]
    [InlineData(Vocabularies_Draft202012.Unevaluated)]
    public void TryGetKeywordSet_ReturnsTrueForKnownVocabulary(string vocab)
    {
        var result = _dialect.TryGetKeywordSet(new Uri(vocab), out var keywords);

        Assert.True(result);
        Assert.NotEmpty(keywords);
    }

    [Fact]
    public void TryGetKeywordSet_ReturnsFalseForUnknownVocabulary()
    {
        var unknown = new Uri("https://example.com/vocab/unknown");

        var result = _dialect.TryGetKeywordSet(unknown, out _);

        Assert.False(result);
    }

    // ── Consistency ─────────────────────────────────────────────────

    [Theory]
    [InlineData(Vocabularies_Draft202012.Core)]
    [InlineData(Vocabularies_Draft202012.Applicator)]
    [InlineData(Vocabularies_Draft202012.Validation)]
    [InlineData(Vocabularies_Draft202012.Metadata)]
    [InlineData(Vocabularies_Draft202012.FormatAnnotation)]
    [InlineData(Vocabularies_Draft202012.FormatAssertion)]
    [InlineData(Vocabularies_Draft202012.Content)]
    [InlineData(Vocabularies_Draft202012.Unevaluated)]
    public void GetKeywordSetAndTryGetKeywordSetReturnSameSet(string vocab)
    {
        var uri = new Uri(vocab);
        var fromGet = _dialect.GetKeywordSet(uri);
        _dialect.TryGetKeywordSet(uri, out var fromTry);

        Assert.Same(fromGet, fromTry);
    }
}

public class Test_Dialect_Draft201909
{
    private readonly Dialect_Draft201909 _dialect = Dialect_Draft201909.Instance;

    // ── GetKeywordSet ───────────────────────────────────────────────

    [Theory]
    [InlineData(Vocabularies_Draft201909.Core)]
    [InlineData(Vocabularies_Draft201909.Applicator)]
    [InlineData(Vocabularies_Draft201909.Validation)]
    [InlineData(Vocabularies_Draft201909.Metadata)]
    [InlineData(Vocabularies_Draft201909.FormatAnnotation)]
    [InlineData(Vocabularies_Draft201909.Content)]
    public void GetKeywordSet_ReturnsNonEmptySetForKnownVocabulary(string vocab)
    {
        var result = _dialect.GetKeywordSet(new Uri(vocab));

        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetKeywordSet_ThrowsForUnknownVocabulary()
    {
        var unknown = new Uri("https://example.com/vocab/unknown");

        Assert.Throws<InvalidOperationException>(() => _dialect.GetKeywordSet(unknown));
    }

    // ── TryGetKeywordSet ────────────────────────────────────────────

    [Theory]
    [InlineData(Vocabularies_Draft201909.Core)]
    [InlineData(Vocabularies_Draft201909.Applicator)]
    [InlineData(Vocabularies_Draft201909.Validation)]
    [InlineData(Vocabularies_Draft201909.Metadata)]
    [InlineData(Vocabularies_Draft201909.FormatAnnotation)]
    [InlineData(Vocabularies_Draft201909.Content)]
    public void TryGetKeywordSet_ReturnsTrueForKnownVocabulary(string vocab)
    {
        var result = _dialect.TryGetKeywordSet(new Uri(vocab), out var keywords);

        Assert.True(result);
        Assert.NotEmpty(keywords);
    }

    [Fact]
    public void TryGetKeywordSet_ReturnsFalseForUnknownVocabulary()
    {
        var unknown = new Uri("https://example.com/vocab/unknown");

        var result = _dialect.TryGetKeywordSet(unknown, out _);

        Assert.False(result);
    }

    // ── Consistency ─────────────────────────────────────────────────

    [Theory]
    [InlineData(Vocabularies_Draft201909.Core)]
    [InlineData(Vocabularies_Draft201909.Applicator)]
    [InlineData(Vocabularies_Draft201909.Validation)]
    [InlineData(Vocabularies_Draft201909.Metadata)]
    [InlineData(Vocabularies_Draft201909.FormatAnnotation)]
    [InlineData(Vocabularies_Draft201909.Content)]
    public void GetKeywordSetAndTryGetKeywordSetReturnSameSet(string vocab)
    {
        var uri = new Uri(vocab);
        var fromGet = _dialect.GetKeywordSet(uri);
        _dialect.TryGetKeywordSet(uri, out var fromTry);

        Assert.Same(fromGet, fromTry);
    }
}
