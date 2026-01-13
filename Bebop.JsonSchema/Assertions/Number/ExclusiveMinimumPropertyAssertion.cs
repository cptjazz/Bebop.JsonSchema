namespace Bebop.JsonSchema.Assertions.Number;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class ExclusiveMinimumPropertyAssertion(double exclusiveMinimum) : NumberPropertyAssertion
{
    public override string[] AssociatedKeyword => ["exclusiveMinimum"];

    protected override bool Assert(double value, in Token element, ErrorCollection errorCollection)
    {
        if (!(value <= exclusiveMinimum)) 
            return true;

        return _AddError(errorCollection, element);

        bool _AddError(ErrorCollection ec, Token e)
        {
            ec.AddError($"Value is not greater than expected exclusive minimum '{exclusiveMinimum}'.", e);
            return false;
        }
    }
}