namespace Bebop.JsonSchema.Assertions;

internal sealed class AndCombinedAssertion(Assertion[] assertions) : Assertion
{
    public static Assertion From(Assertion[] assertions)
    {
        if (assertions.Length == 0)
            return AnyTypeAssertion.Instance;
        
        if (assertions.Length == 1)
            return assertions[0];
        
        return new AndCombinedAssertion(assertions);
    }

    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        var isValid = true;
        await SyncContext.Drop();

        foreach (var assertion in assertions)
        {
            if (!await assertion.Assert(element, evaluationState, errorCollection))
            {
                isValid = false;
            }
        }

        return isValid;
    }

    public override async ValueTask PrepareImpl()
    {
        await SyncContext.Drop();

        foreach (var assertion in assertions)
        {
            await assertion.Prepare();
        }
    }
}