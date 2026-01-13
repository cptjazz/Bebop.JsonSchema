namespace Bebop.JsonSchema.Assertions.Type;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal abstract class TypeAssertion : Assertion
{
    public sealed override string[] AssociatedKeyword => ["type"];

    public sealed override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    { 
         return Assert(element, errorCollection);
    }

    public abstract bool Assert(in Token element, ErrorCollection errorCollection);
}