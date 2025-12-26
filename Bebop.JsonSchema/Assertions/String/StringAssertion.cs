namespace Bebop.JsonSchema.Assertions.String;

internal abstract class StringAssertion : PreparedAssertion
{
    public sealed override ValueTask<bool> Assert(Token element, EvaluationState evaluationState,
        ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind == JsonValueKind.String)
            return ValueTask.FromResult(AssertProperty(element.Element.GetString()!, element, errorCollection));

        // Non-strings shall be ignored
        return ValueTask.FromResult(true);
    }

    public abstract bool AssertProperty(string text, in Token element, ErrorCollection errorCollection);
}