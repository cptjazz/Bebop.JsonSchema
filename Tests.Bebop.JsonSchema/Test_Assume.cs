namespace Tests.MyJsonSchema;

public class Test_Assume
{
    // ── That(true) ──────────────────────────────────────────────────

    [Fact]
    public void That_WhenTrue_OtherwiseThrowDoesNotThrow()
    {
        var assumption = Assume.That(true);

        assumption.OtherwiseThrow(() => new InvalidOperationException("should not be thrown"));
    }

    [Fact]
    public void That_WhenTrue_ReturnsTrueAssumption()
    {
        var assumption = Assume.That(true);

        Assert.IsType<Assume.TrueAssumption>(assumption);
    }

    [Fact]
    public void That_WhenTrue_ReturnsSameInstance()
    {
        var first = Assume.That(true);
        var second = Assume.That(true);

        Assert.Same(first, second);
    }

    // ── That(false) ─────────────────────────────────────────────────

    [Fact]
    public void That_WhenFalse_OtherwiseThrowThrowsProvidedException()
    {
        var assumption = Assume.That(false);

        var ex = Assert.Throws<InvalidOperationException>(
            () => assumption.OtherwiseThrow(() => new InvalidOperationException("test message")));

        Assert.Equal("test message", ex.Message);
    }

    [Fact]
    public void That_WhenFalse_ReturnsFalseAssumption()
    {
        var assumption = Assume.That(false);

        Assert.IsType<Assume.FalseAssumption>(assumption);
    }

    [Fact]
    public void That_WhenFalse_ReturnsSameInstance()
    {
        var first = Assume.That(false);
        var second = Assume.That(false);

        Assert.Same(first, second);
    }

    [Fact]
    public void That_WhenFalse_OtherwiseThrowThrowsExactExceptionType()
    {
        var assumption = Assume.That(false);

        Assert.Throws<ArgumentException>(
            () => assumption.OtherwiseThrow(() => new ArgumentException("arg error")));
    }
}
