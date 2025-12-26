namespace Bebop.JsonSchema.Assertions.Array;

internal abstract class ArrayAssertion : PreparedAssertion
{
    public override ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind == JsonValueKind.Array) 
            return ValueTask.FromResult(Assert(element, errorCollection));

        // Non-arrays shall be ignored
        return ValueTask.FromResult(true);
    }

    public abstract bool Assert(in Token array, ErrorCollection errorCollection);
}