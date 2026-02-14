using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Object;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class AdditionalPropertiesAssertion(JsonSchema schema) : Assertion
{
    public override int Order => 99;

    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        JsonElement el = element.Element;
        if (el.ValueKind != JsonValueKind.Object)
        {
            // Non-objects shall be ignored
            return true;
        }

        var isValid = true;

        foreach (var property in el.EnumerateObject())
        {
            var propertyPath = element.ElementPath.AppendPropertyName(property.Name);
            if (evaluationState.HasSeenProperty(propertyPath))
                continue;

            var propertyValue = property.Value;
            var h = new Token(in propertyValue, propertyPath);
            if (!await schema.Validate(h, evaluationState, errorCollection).ConfigureAwait(false))
            {
                _AddError(errorCollection, property, h);

                isValid = false;
            }

            evaluationState.AddProperty(propertyPath, isValid);
        }

        return isValid;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void _AddError(ErrorCollection ec, in JsonProperty p, in Token h)
        {
            ec.AddError($"Additional property '{p.Name}' does not match the schema defined by 'additionalProperties'.",
                        h);
        }
    }

    public override ValueTask PrepareImpl()
    {
        return schema.Prepare();
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => "additionalProperties";
}
