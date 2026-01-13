namespace Bebop.JsonSchema.Assertions.String;

internal abstract class StringAssertion : Assertion
{
    public sealed override bool Assert(in Token element, in EvaluationState evaluationState,
        ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind == JsonValueKind.String)
            return AssertProperty(element.Element.GetString()!, element, errorCollection);

        // Non-strings shall be ignored
        return true;

    }

    public abstract bool AssertProperty(string text, in Token element, ErrorCollection errorCollection);
}