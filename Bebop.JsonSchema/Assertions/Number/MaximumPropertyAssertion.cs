namespace Bebop.JsonSchema.Assertions.Number;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class MaximumPropertyAssertion(double maximum) : NumberPropertyAssertion
{
    public override string[] AssociatedKeyword => ["maximum"];

    protected override bool Assert(double value, in Token element, ErrorCollection errorCollection)
    {
        if (value <= maximum) 
            return true;

        return _AddError(errorCollection, element);

        bool _AddError(ErrorCollection ec, Token e)
        {
            ec.AddError($"Value is greater than expected maximum '{maximum}'.", e);
            return false;
        }
    }
}