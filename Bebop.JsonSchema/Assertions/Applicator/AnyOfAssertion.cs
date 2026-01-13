namespace Bebop.JsonSchema.Assertions.Applicator;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class AnyOfAssertion(JsonSchema[] schemas) : Assertion
{
    public override string[] AssociatedKeyword => ["anyOf"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        var l = schemas.Length;

        if (l == 0)
            return true;

        var isValid = false;
        using var absorptionList = Pool.RentArray<EvaluationState>(l);

        foreach (var schema in schemas)
        {
            var es = evaluationState.New();
            if (schema.Validate(element, es, errorCollection))
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

    public override async ValueTask Prepare()
    {
        foreach (var schema in schemas)
        {
            await schema
                .Prepare()
                .ConfigureAwait(false);
        }
    }
}