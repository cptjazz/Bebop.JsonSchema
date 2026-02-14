namespace Bebop.JsonSchema.Assertions.Object;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class PropertyNamesAssertion(JsonSchema schema) : Assertion
{
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
            var p = JsonSerializer.SerializeToElement(property.Name);
                
            if (!await schema.Validate(element.Subitem(in p, property.Name), evaluationState, errorCollection).ConfigureAwait(false))
            {
                _AddError(element, errorCollection, property);
                isValid = false;
            }
        }

        return isValid;

        static void _AddError(in Token e, ErrorCollection ec, in JsonProperty p)
        {
            ec.AddError($"Property name '{p.Name}' does not match the schema.", e);
        }
    }

    public override ValueTask PrepareImpl()
    {
        return schema.Prepare();
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => "propertyNames";
}
