namespace Bebop.JsonSchema.Assertions.Array;

internal abstract class ArrayAssertion : Assertion
{
    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind == JsonValueKind.Array) 
            return Assert(element, errorCollection);

        // Non-arrays shall be ignored
        return true;
    }

    public abstract bool Assert(in Token array, ErrorCollection errorCollection);
}