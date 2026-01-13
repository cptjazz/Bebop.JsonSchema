using Bebop.JsonSchema.Assertions.Type;

namespace Bebop.JsonSchema.Assertions;

internal sealed class AndCombinedAssertion(Assertion[] assertions) : Assertion
{
    public override string[] AssociatedKeyword => [];

    public static Assertion From(Assertion[] assertions)
    {
        if (assertions.Length == 0)
            return AnyTypeAssertion.Instance;
        
        if (assertions.Length == 1)
            return assertions[0];
        
        return new AndCombinedAssertion(assertions);
    }

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        var isValid = true;

        foreach (var assertion in assertions)
        {
            if (!assertion.Assert(element, evaluationState, errorCollection))
            {
                isValid = false;
            }
        }

        return isValid;
    }

    public override async ValueTask Prepare()
    {
        foreach (var assertion in assertions)
        {
            await assertion
                .Prepare()
                .ConfigureAwait(false);
        }
    }
}