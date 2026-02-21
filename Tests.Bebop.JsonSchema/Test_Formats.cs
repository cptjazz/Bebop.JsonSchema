using Bebop.JsonSchema.Assertions;

namespace Tests.MyJsonSchema;

public class Test_Formats
{
    // ── Get ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData("email")]
    [InlineData("idn-email")]
    [InlineData("ipv4")]
    [InlineData("ipv6")]
    [InlineData("uri")]
    [InlineData("uuid")]
    [InlineData("date-time")]
    [InlineData("date")]
    [InlineData("time")]
    [InlineData("duration")]
    public void Get_ReturnsValidatorForKnownFormat(string format)
    {
        var validator = Formats.Get(format);

        Assert.NotNull(validator);
    }

    [Fact]
    public void Get_ReturnsAlwaysTrueForUnknownFormat()
    {
        var validator = Formats.Get("unknown-format");

        Assert.True(validator("anything"));
        Assert.True(validator(""));
    }

    // ── email ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("user+tag@example.com", true)]
    [InlineData("not-an-email", false)]
    [InlineData("@missing-local.com", false)]
    [InlineData("missing-at-sign", false)]
    public void WhenFormatIsEmailThenValidationMatchesExpected(string input, bool expected)
    {
        var validator = Formats.Get("email");

        Assert.Equal(expected, validator(input));
    }

    // ── ipv4 ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("127.0.0.1", true)]
    [InlineData("192.168.1.1", true)]
    [InlineData("999.999.999.999", false)]
    [InlineData("not-an-ip", false)]
    [InlineData("::1", false)]
    public void WhenFormatIsIpv4ThenValidationMatchesExpected(string input, bool expected)
    {
        var validator = Formats.Get("ipv4");

        Assert.Equal(expected, validator(input));
    }

    // ── ipv6 ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("::1", true)]
    [InlineData("2001:db8::1", true)]
    [InlineData("127.0.0.1", false)]
    [InlineData("not-an-ip", false)]
    public void WhenFormatIsIpv6ThenValidationMatchesExpected(string input, bool expected)
    {
        var validator = Formats.Get("ipv6");

        Assert.Equal(expected, validator(input));
    }

    // ── uri ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("http://example.com/path?q=1", true)]
    [InlineData("not a uri", false)]
    [InlineData("relative/path", false)]
    public void WhenFormatIsUriThenValidationMatchesExpected(string input, bool expected)
    {
        var validator = Formats.Get("uri");

        Assert.Equal(expected, validator(input));
    }

    // ── uuid ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("550e8400-e29b-41d4-a716-446655440000", true)]
    [InlineData("not-a-uuid", false)]
    [InlineData("", false)]
    public void WhenFormatIsUuidThenValidationMatchesExpected(string input, bool expected)
    {
        var validator = Formats.Get("uuid");

        Assert.Equal(expected, validator(input));
    }

    // ── date-time ───────────────────────────────────────────────────

    [Theory]
    [InlineData("2024-01-15T10:30:00Z", true)]
    [InlineData("2024-01-15T10:30:00+01:00", true)]
    [InlineData("not-a-date-time", false)]
    public void WhenFormatIsDateTimeThenValidationMatchesExpected(string input, bool expected)
    {
        var validator = Formats.Get("date-time");

        Assert.Equal(expected, validator(input));
    }

    // ── date ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("2024-01-15", true)]
    [InlineData("not-a-date", false)]
    public void WhenFormatIsDateThenValidationMatchesExpected(string input, bool expected)
    {
        var validator = Formats.Get("date");

        Assert.Equal(expected, validator(input));
    }

    // ── time ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("10:30:00", true)]
    [InlineData("23:59:59", true)]
    [InlineData("not-a-time", false)]
    public void WhenFormatIsTimeThenValidationMatchesExpected(string input, bool expected)
    {
        var validator = Formats.Get("time");

        Assert.Equal(expected, validator(input));
    }

    // ── duration ────────────────────────────────────────────────────

    [Theory]
    [InlineData("P1Y2M3D", true)]
    [InlineData("PT1H30M", true)]
    [InlineData("P1D", true)]
    [InlineData("not-a-duration", false)]
    [InlineData("", false)]
    public void WhenFormatIsDurationThenValidationMatchesExpected(string input, bool expected)
    {
        var validator = Formats.Get("duration");

        Assert.Equal(expected, validator(input));
    }

    // ── idn-email ───────────────────────────────────────────────────

    [Fact]
    public void WhenFormatIsIdnEmailThenAlwaysReturnsTrue()
    {
        var validator = Formats.Get("idn-email");

        Assert.True(validator("anything"));
        Assert.True(validator("user@例え.jp"));
    }
}
