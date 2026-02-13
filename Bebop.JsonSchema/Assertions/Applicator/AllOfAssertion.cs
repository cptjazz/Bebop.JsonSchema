namespace Bebop.JsonSchema.Assertions.Applicator;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class AllOfAssertion(JsonSchema[] schemas) : Assertion
{
    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        var isValid = true;
        using var absorptionList = Pool.RentArray<EvaluationState>(schemas.Length);

        foreach (var schema in schemas)
        {
            var es = evaluationState.New();
            if (!await schema.Validate(element, es, errorCollection).ConfigureAwait(false))
            {
                errorCollection.AddError("Element does not match all schemas in 'allOf' assertion.", element);
                isValid = false;
            }
            else
            {
                absorptionList.Add(es);
            }
        }

        foreach (var es in absorptionList.AsSpan())
        {
            evaluationState.Absorb(es);
        }

        return isValid;
    }

    public override async ValueTask PrepareImpl()
    {
        await SyncContext.Drop();
        
        foreach (var schema in schemas)
        {
            await schema.Prepare();
        }
    }
}