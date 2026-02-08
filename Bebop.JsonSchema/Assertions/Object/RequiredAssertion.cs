using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Object;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class RequiredAssertion(string[] requiredProperties) : Assertion
{
    public override string[] AssociatedKeyword => ["required"];

    public override ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        JsonElement el = element.Element;
        if (el.ValueKind != JsonValueKind.Object)
        {
            // Non-objects shall be ignored
            return ValueTask.FromResult(true);
        }

        bool isValid = true;

        foreach (var propertyName in requiredProperties)
        {
            if (!el.TryGetProperty(propertyName, out _))
            {
                _AddError(element, errorCollection, propertyName);
                isValid = false;
            }
        }

        return ValueTask.FromResult(isValid);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void _AddError(in Token e, ErrorCollection ec, string p)
        {
            ec.AddError($"Object is missing required property '{p}'", e);
        }
    }
}