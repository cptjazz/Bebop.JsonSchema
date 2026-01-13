namespace Bebop.JsonSchema.Assertions.Applicator;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class ConditionalAssertion(JsonSchema ifSchema, JsonSchema thenSchema, JsonSchema elseSchema)
    : Assertion
{
    public override string[] AssociatedKeyword => ["if", "then", "else"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        return ifSchema.Validate(element, evaluationState, errorCollection) 
            ? thenSchema.Validate(element, evaluationState, errorCollection) 
            : elseSchema.Validate(element, evaluationState, errorCollection);
    }
}