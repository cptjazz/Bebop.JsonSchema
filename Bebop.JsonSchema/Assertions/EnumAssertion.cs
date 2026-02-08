using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class EnumAssertion(JsonElement[] elements) : PreparedAssertion
{
    public override string[] AssociatedKeyword => ["enum"];

    public override ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        JsonElement el = element.Element;

        foreach (var value in elements)
        {
            if (JsonComparer.Instance.Equals2(el, value))
            {
                return ValueTask.FromResult(true);
            }
        }

        return ValueTask.FromResult(_AddError(element, errorCollection));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool _AddError(in Token e, ErrorCollection ec)
        {
            ec.AddError("Element does not match any value in the enum.", e);
            return false;
        }
    }
}