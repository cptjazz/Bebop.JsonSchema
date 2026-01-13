namespace Bebop.JsonSchema.Assertions.Type;

internal sealed class NoneTypeAssertion : TypeAssertion
{
    public static NoneTypeAssertion Instance { get; } = new();

    public override bool Assert(in Token element, ErrorCollection errorCollection)
    {
        errorCollection.AddError("Value does not match any allowed type.", element);
        return false;
    }
}