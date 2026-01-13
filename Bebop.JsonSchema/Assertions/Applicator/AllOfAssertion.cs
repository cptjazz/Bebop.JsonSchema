namespace Bebop.JsonSchema.Assertions.Applicator;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class AllOfAssertion(JsonSchema[] schemas) : Assertion
{
    public override string[] AssociatedKeyword => ["allOf"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        var isValid = true;
        using var absorptionList = Pool.RentArray<EvaluationState>(schemas.Length);

        foreach (var schema in schemas)
        {
            var es = evaluationState.New();
            if (!schema.Validate(element, es, errorCollection))
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

    public override async ValueTask Prepare()
    {
        foreach (var s in schemas)
        {
            await s
                .Prepare()
                .ConfigureAwait(false);
        }
    }
}