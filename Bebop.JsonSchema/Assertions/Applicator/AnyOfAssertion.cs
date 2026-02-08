namespace Bebop.JsonSchema.Assertions.Applicator;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class AnyOfAssertion(JsonSchema[] schemas) : Assertion
{
    public override string[] AssociatedKeyword => ["anyOf"];

    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        var l = schemas.Length;

        if (l == 0)
            return true;

        var isValid = false;
        using var absorptionList = Pool.RentArray<EvaluationState>(l);

        foreach (var schema in schemas)
        {
            var es = evaluationState.New();
            if (await schema.Validate(element, es, errorCollection).ConfigureAwait(false))
            {
                isValid = true;
                absorptionList.Add(es);
            }
        }

        if (!isValid)
        {
            errorCollection.AddError("Element does not match any schema in 'anyOf' assertion.", element);
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