using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class EnumAssertion(JsonElement[] elements) : Assertion
{
    public override string[] AssociatedKeyword => ["enum"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        JsonElement el = element.Element;

        foreach (var value in elements)
        {
            if (JsonComparer.Instance.Equals2(el, value))
            {
                return true;
            }
        }

        return _AddError(element, errorCollection);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool _AddError(in Token e, ErrorCollection ec)
        {
            ec.AddError("Element does not match any value in the enum.", e);
            return false;
        }
    }
}