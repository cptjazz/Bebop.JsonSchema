namespace Bebop.JsonSchema.Assertions.Type;

internal sealed class AnyTypeAssertion : TypeAssertion
{
    public static AnyTypeAssertion Instance { get; } = new();

    public override ValueTask<bool> Assert(in Token element, ErrorCollection errorCollection)
    {
        return ValueTask.FromResult(true);
    }
}