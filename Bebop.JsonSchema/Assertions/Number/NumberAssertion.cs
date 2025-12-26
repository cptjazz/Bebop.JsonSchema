namespace Bebop.JsonSchema.Assertions.Number;

internal abstract class NumberAssertion : PreparedAssertion
{
    public override ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind == JsonValueKind.Number)
            return ValueTask.FromResult(Assert(element.Element.GetDouble(), element, errorCollection));

        // Non-numbers shall be ignored
        return ValueTask.FromResult(true);
    }

    protected abstract bool Assert(double value, in Token element, ErrorCollection errorCollection);
}