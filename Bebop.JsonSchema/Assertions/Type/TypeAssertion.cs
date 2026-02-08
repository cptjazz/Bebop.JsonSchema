namespace Bebop.JsonSchema.Assertions.Type;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal abstract class TypeAssertion : PreparedAssertion
{
    public sealed override string[] AssociatedKeyword => ["type"];

    public sealed override ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    { 
         return Assert(element, errorCollection);
    }

    public abstract ValueTask<bool> Assert(in Token element, ErrorCollection errorCollection);
}