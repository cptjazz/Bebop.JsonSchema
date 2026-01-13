namespace Bebop.JsonSchema.Assertions.Object;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class PropertyNamesAssertion(JsonSchema schema) : Assertion
{
    public override string[] AssociatedKeyword => ["propertyNames"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
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
                
            if (!schema.Validate(element.Subitem(in p, property.Name), evaluationState, errorCollection))
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
}