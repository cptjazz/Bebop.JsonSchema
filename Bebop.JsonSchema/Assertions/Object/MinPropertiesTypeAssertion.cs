using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Object;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class MinPropertiesAssertion(int minProperties) : Assertion
{
    public override string[] AssociatedKeyword => ["minProperties"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        JsonElement el = element.Element;
        if (el.ValueKind != JsonValueKind.Object)
            return true;

        if (el.GetPropertyCount() < minProperties)
        {
            _AddError(element, errorCollection, minProperties);
            return false;
        }

        return true;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void _AddError(in Token e, ErrorCollection ec, int m)
        {
            ec.AddError($"Object must have at least {m} properties", e);
        }
    }
}