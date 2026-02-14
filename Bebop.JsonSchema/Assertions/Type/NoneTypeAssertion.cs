namespace Bebop.JsonSchema.Assertions.Type;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class NoneTypeAssertion : TypeAssertion
{
    public static NoneTypeAssertion Instance { get; } = new();

    public override ValueTask<bool> Assert(in Token element, ErrorCollection errorCollection)
    {
        errorCollection.AddError("Value does not match any allowed type.", element);
        return ValueTask.FromResult(false);
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => "nothing";
}
