namespace Bebop.JsonSchema.Assertions.Number;

internal abstract class NumberPropertyAssertion : Assertion
{
    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind == JsonValueKind.Number)
            return Assert(element.Element.GetDouble(), element, errorCollection);

        // Non-numbers shall be ignored
        return true;
    }

    protected abstract bool Assert(double value, in Token element, ErrorCollection errorCollection);
}