using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Object;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class MaxPropertiesPropertyAssertion(int maxProperties) : Assertion
{
    public override string[] AssociatedKeyword => ["maxProperties"];

    public override ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        JsonElement el = element.Element;
        if (el.ValueKind != JsonValueKind.Object)
            return ValueTask.FromResult(true);

        if (el.GetPropertyCount() > maxProperties)
        {
            _AddError(element, errorCollection, maxProperties);
            return ValueTask.FromResult(false);
        }

        return ValueTask.FromResult(true);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void _AddError(in Token e, ErrorCollection ec, int m)
        {
            ec.AddError($"Object must have at most {m} properties", e);
        }
    }
}