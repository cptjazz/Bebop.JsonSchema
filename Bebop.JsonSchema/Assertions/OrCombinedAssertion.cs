namespace Bebop.JsonSchema.Assertions;

internal sealed class OrCombinedAssertion(Assertion[] assertions) : Assertion
{
    public override string[] AssociatedKeyword => [];

    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        for (var i = 0; i < assertions.Length; i++)
        {
            if (await assertions[i].Assert(element, evaluationState, errorCollection).ConfigureAwait(false))
                return true;
        }

        return false;
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