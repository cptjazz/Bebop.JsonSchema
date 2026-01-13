namespace Bebop.JsonSchema.Assertions;

internal sealed class OrCombinedAssertion(Assertion[] assertions) : Assertion
{
    public override string[] AssociatedKeyword => [];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        for (var i = 0; i < assertions.Length; i++)
        {
            if (assertions[i].Assert(element, evaluationState, errorCollection))
                return true;
        }

        return false;
    }
}