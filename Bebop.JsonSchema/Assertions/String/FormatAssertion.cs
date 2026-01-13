namespace Bebop.JsonSchema.Assertions.String;

internal sealed class FormatAssertion(Func<string, bool> predicate) : StringAssertion
{
    public override string[] AssociatedKeyword => ["format"];

    public override bool AssertProperty(string text, in Token element, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind is not JsonValueKind.String)
        {
            return true;
        }

        if (predicate(text))
        {
            return true;
        }

        return _AddError(element, errorCollection);

        static bool _AddError(in Token e, ErrorCollection ec)
        {
            ec.AddError($"String does not match the required format.", in e);
            return false;
        }
    }
}
