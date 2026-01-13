namespace Bebop.JsonSchema.Assertions.Number;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class ExclusiveMaximumPropertyAssertion(double exclusiveMaximum) : NumberPropertyAssertion
{
    public override string[] AssociatedKeyword => ["exclusiveMaximum"];

    protected override bool Assert(double value, in Token element, ErrorCollection errorCollection)
    {
        if (!(value >= exclusiveMaximum)) 
            return true;

        return _AddError(element, errorCollection);

        bool _AddError(in Token e, ErrorCollection ec)
        {
            ec.AddError($"Value is not less than expected exclusive maximum '{exclusiveMaximum}'.", e);
            return false;
        }
    }
}