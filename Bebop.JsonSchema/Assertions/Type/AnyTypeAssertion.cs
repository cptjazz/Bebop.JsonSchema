namespace Bebop.JsonSchema.Assertions.Type;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class AnyTypeAssertion : TypeAssertion
{
    public static AnyTypeAssertion Instance { get; } = new();

    public override ValueTask<bool> Assert(in Token element, ErrorCollection errorCollection)
    {
        return ValueTask.FromResult(true);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => "anything";
}