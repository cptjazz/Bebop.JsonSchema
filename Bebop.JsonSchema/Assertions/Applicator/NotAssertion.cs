namespace Bebop.JsonSchema.Assertions.Applicator;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class NotAssertion(JsonSchema schema) : Assertion
{
    public override string[] AssociatedKeyword => ["not"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        var es = evaluationState.New();
        var result = schema.Validate(element, es, NopErrorCollection.Instance);

        if (!result)
        {
            evaluationState.Absorb(es);
            return true;
        }

        errorCollection.AddError("Element matches schema in 'not' assertion.", element);
        return false;
    }
}