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

public class Test_CustomDialect
{
    // ── GetKeywordSet ───────────────────────────────────────────────

    [Theory]
    [InlineData(Vocabularies_Draft202012.Core)]
    [InlineData(Vocabularies_Draft202012.Applicator)]
    [InlineData(Vocabularies_Draft202012.Validation)]
    public void GetKeywordSet_DelegatesToBaseDialect(string vocab)
    {
        var dialect = CreateDialect(new Uri(vocab));
        var expected = Dialect_Draft202012.Instance.GetKeywordSet(new Uri(vocab));

        var result = dialect.GetKeywordSet(new Uri(vocab));

        Assert.Same(expected, result);
    }

    [Fact]
    public void GetKeywordSet_ThrowsForUnknownVocabulary()
    {
        var dialect = CreateDialect(new Uri(Vocabularies_Draft202012.Core));
        var unknown = new Uri("https://example.com/vocab/unknown");

        Assert.Throws<InvalidOperationException>(() => dialect.GetKeywordSet(unknown));
    }

    // ── TryGetKeywordSet ────────────────────────────────────────────

    [Theory]
    [InlineData(Vocabularies_Draft202012.Core)]
    [InlineData(Vocabularies_Draft202012.Applicator)]
    [InlineData(Vocabularies_Draft202012.Validation)]
    public void TryGetKeywordSet_ReturnsTrueForKnownVocabulary(string vocab)
    {
        var dialect = CreateDialect(new Uri(vocab));

        var result = dialect.TryGetKeywordSet(new Uri(vocab), out var keywords);

        Assert.True(result);
        Assert.NotEmpty(keywords);
    }

    [Fact]
    public void TryGetKeywordSet_ReturnsFalseForUnknownVocabulary()
    {
        var dialect = CreateDialect(new Uri(Vocabularies_Draft202012.Core));
        var unknown = new Uri("https://example.com/vocab/unknown");

        var result = dialect.TryGetKeywordSet(unknown, out _);

        Assert.False(result);
    }

    // ── Consistency ─────────────────────────────────────────────────

    [Theory]
    [InlineData(Vocabularies_Draft202012.Core)]
    [InlineData(Vocabularies_Draft202012.Applicator)]
    [InlineData(Vocabularies_Draft202012.Validation)]
    public void GetKeywordSetAndTryGetKeywordSetReturnSameSet(string vocab)
    {
        var dialect = CreateDialect(new Uri(vocab));
        var uri = new Uri(vocab);

        var fromGet = dialect.GetKeywordSet(uri);
        dialect.TryGetKeywordSet(uri, out var fromTry);

        Assert.Same(fromGet, fromTry);
    }

    // ── SupportedKeywords from vocabulary ───────────────────────────

    [Fact]
    public void SupportedKeywords_ContainsOnlyKeywordsFromDeclaredVocabularies()
    {
        var coreUri = new Uri(Vocabularies_Draft202012.Core);
        var dialect = CreateDialect(coreUri);

        var expected = Dialect_Draft202012.Instance.GetKeywordSet(coreUri);

        Assert.Equal(expected.Count, dialect.SupportedKeywords.Count);
        Assert.True(expected.SetEquals(dialect.SupportedKeywords));
    }

    [Fact]
    public void SupportedKeywords_UnionOfMultipleVocabularies()
    {
        var coreUri = new Uri(Vocabularies_Draft202012.Core);
        var validationUri = new Uri(Vocabularies_Draft202012.Validation);
        var dialect = CreateDialect(coreUri, validationUri);

        var coreKeywords = Dialect_Draft202012.Instance.GetKeywordSet(coreUri);
        var validationKeywords = Dialect_Draft202012.Instance.GetKeywordSet(validationUri);

        foreach (var kw in coreKeywords)
            Assert.Contains(kw, dialect.SupportedKeywords);

        foreach (var kw in validationKeywords)
            Assert.Contains(kw, dialect.SupportedKeywords);
    }

    [Fact]
    public void SupportedKeywords_EmptyWhenNoVocabulary()
    {
        var dialect = CreateDialect();

        Assert.Empty(dialect.SupportedKeywords);
    }

    // ── IsFormatAssertion ───────────────────────────────────────────

    [Fact]
    public void IsFormatAssertion_FalseByDefault()
    {
        var dialect = CreateDialect(new Uri(Vocabularies_Draft202012.Core));

        Assert.False(dialect.IsFormatAssertion);
    }

    [Fact]
    public void IsFormatAssertion_TrueWhenFormatAssertionVocabularyPresent()
    {
        var dialect = CreateDialect(
            new Uri(Vocabularies_Draft202012.Core),
            new Uri(Vocabularies_Draft202012.FormatAssertion));

        Assert.True(dialect.IsFormatAssertion);
    }

    [Fact]
    public void IsFormatAssertion_FalseWhenOnlyFormatAnnotationPresent()
    {
        var dialect = CreateDialect(
            new Uri(Vocabularies_Draft202012.Core),
            new Uri(Vocabularies_Draft202012.FormatAnnotation));

        Assert.False(dialect.IsFormatAssertion);
    }

    // ── Unknown vocabulary handling ─────────────────────────────────

    [Fact]
    public void Constructor_SkipsUnknownVocabularies()
    {
        var coreUri = new Uri(Vocabularies_Draft202012.Core);
        var unknownUri = new Uri("https://example.com/vocab/custom");
        var dialect = CreateDialect(coreUri, unknownUri);

        var coreKeywords = Dialect_Draft202012.Instance.GetKeywordSet(coreUri);

        Assert.Equal(coreKeywords.Count, dialect.SupportedKeywords.Count);
        Assert.True(coreKeywords.SetEquals(dialect.SupportedKeywords));
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private static CustomDialect CreateDialect(params Uri[] vocabularyUris)
    {
        var repo = SchemaRegistry.Local();
        var baseDialect = Dialect_Draft202012.Instance;
        var schema = new JsonSchema(
            new Uri("https://example.com/test-meta"), repo, new Anchors(), false, baseDialect);

        if (vocabularyUris.Length > 0)
        {
            schema.Vocabulary = new HashSet<Uri>(vocabularyUris);
        }

        return new CustomDialect(baseDialect, schema);
    }
}
