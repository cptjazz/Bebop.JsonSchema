using Bebop.JsonSchema.Assertions.String;

namespace Tests.MyJsonSchema;

public class Test_RegexNormalizer
{
    // ── Positive category (\p{}) ────────────────────────────────────

    [Theory]
    [InlineData(@"\p{Digit}", @"\p{Nd}")]
    [InlineData(@"\p{DecimalNumber}", @"\p{Nd}")]
    [InlineData(@"\p{Letter}", @"\p{L}")]
    [InlineData(@"\p{UppercaseLetter}", @"\p{Lu}")]
    [InlineData(@"\p{LowercaseLetter}", @"\p{Ll}")]
    [InlineData(@"\p{TitlecaseLetter}", @"\p{Lt}")]
    [InlineData(@"\p{ModifierLetter}", @"\p{Lm}")]
    [InlineData(@"\p{OtherLetter}", @"\p{Lo}")]
    [InlineData(@"\p{Number}", @"\p{N}")]
    [InlineData(@"\p{LetterNumber}", @"\p{Nl}")]
    [InlineData(@"\p{OtherNumber}", @"\p{No}")]
    [InlineData(@"\p{Punctuation}", @"\p{P}")]
    [InlineData(@"\p{Symbol}", @"\p{S}")]
    [InlineData(@"\p{Separator}", @"\p{Z}")]
    [InlineData(@"\p{Control}", @"\p{Cc}")]
    [InlineData(@"\p{Format}", @"\p{Cf}")]
    [InlineData(@"\p{PrivateUse}", @"\p{Co}")]
    [InlineData(@"\p{Surrogate}", @"\p{Cs}")]
    [InlineData(@"\p{Unassigned}", @"\p{Cn}")]
    public void WhenPositiveCategoryThenNormalized(string input, string expected)
    {
        Assert.Equal(expected, RegexNormalizer.Normalize(input));
    }

    // ── Negated category (\P{}) ─────────────────────────────────────

    [Theory]
    [InlineData(@"\P{Digit}", @"\P{Nd}")]
    [InlineData(@"\P{Letter}", @"\P{L}")]
    [InlineData(@"\P{Number}", @"\P{N}")]
    [InlineData(@"\P{Punctuation}", @"\P{P}")]
    [InlineData(@"\P{Control}", @"\P{Cc}")]
    public void WhenNegatedCategoryThenNormalized(string input, string expected)
    {
        Assert.Equal(expected, RegexNormalizer.Normalize(input));
    }

    // ── Case insensitivity ──────────────────────────────────────────

    [Theory]
    [InlineData(@"\p{digit}", @"\p{Nd}")]
    [InlineData(@"\p{DIGIT}", @"\p{Nd}")]
    [InlineData(@"\p{DiGiT}", @"\p{Nd}")]
    [InlineData(@"\P{letter}", @"\P{L}")]
    [InlineData(@"\P{LETTER}", @"\P{L}")]
    public void WhenCategoryNameDiffersCaseThenStillNormalized(string input, string expected)
    {
        Assert.Equal(expected, RegexNormalizer.Normalize(input));
    }

    // ── No-op for already-normalized or unknown categories ──────────

    [Theory]
    [InlineData(@"\p{Nd}")]
    [InlineData(@"\p{L}")]
    [InlineData(@"\p{Lu}")]
    [InlineData(@"\P{Nd}")]
    [InlineData(@"[a-z]+")]
    [InlineData(@"^\d{3}-\d{4}$")]
    [InlineData(@"")]
    public void WhenAlreadyNormalizedOrPlainThenUnchanged(string input)
    {
        Assert.Equal(input, RegexNormalizer.Normalize(input));
    }

    // ── Multiple categories in one pattern ──────────────────────────

    [Fact]
    public void WhenMultipleCategoriesThenAllNormalized()
    {
        var input = @"[\p{Letter}\p{Digit}]+";
        var expected = @"[\p{L}\p{Nd}]+";
        Assert.Equal(expected, RegexNormalizer.Normalize(input));
    }

    [Fact]
    public void WhenMixedPositiveAndNegatedThenAllNormalized()
    {
        var input = @"\p{Letter}\P{Number}";
        var expected = @"\p{L}\P{N}";
        Assert.Equal(expected, RegexNormalizer.Normalize(input));
    }

    // ── Embedded in larger patterns ─────────────────────────────────

    [Fact]
    public void WhenCategoryEmbeddedInComplexPatternThenOnlyCategoryReplaced()
    {
        var input = @"^[a-z\p{Digit}]+@[a-z\p{Letter}]+\.[a-z]{2,}$";
        var expected = @"^[a-z\p{Nd}]+@[a-z\p{L}]+\.[a-z]{2,}$";
        Assert.Equal(expected, RegexNormalizer.Normalize(input));
    }

    [Fact]
    public void WhenPatternHasNoCategoriesThenUnchanged()
    {
        var input = @"^https?://[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        Assert.Equal(input, RegexNormalizer.Normalize(input));
    }
}
