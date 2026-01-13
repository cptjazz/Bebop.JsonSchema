namespace Bebop.JsonSchema.Assertions.Type;

internal sealed class AnyTypeAssertion : TypeAssertion
{
    public static AnyTypeAssertion Instance { get; } = new();

    public override bool Assert(in Token element, ErrorCollection errorCollection)
    {
        return true;
    }
}