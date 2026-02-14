namespace Bebop.JsonSchema.Assertions.Type;

internal abstract class TypeAssertion : PreparedAssertion
{
    public sealed override ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    { 
         return Assert(element, errorCollection);
    }

    public abstract ValueTask<bool> Assert(in Token element, ErrorCollection errorCollection);
}
