namespace Bebop.JsonSchema.Assertions.Object;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Unevaluated)]
internal sealed class UnevaluatedPropertiesAssertion(JsonSchema schema) : Assertion
{
    public override string[] AssociatedKeyword => ["unevaluatedProperties"];

    public override int Order => 1000;

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind != JsonValueKind.Object)
            return true;

        var isValid = true;
        
        foreach (var property in element.Element.EnumerateObject())
        {
            var propertyPath = element.ElementPath.AppendPropertyName(property.Name);

            if (evaluationState.IsPropertyUnevaluated(propertyPath))
            {
                var value = property.Value;
                var h = new Token(in value, propertyPath);
                if (!schema.Validate(h, evaluationState, errorCollection))
                {
                    errorCollection.AddError($"Property '{property.Name}' does not match schema in 'unevaluatedProperties' assertion.", h);
                    isValid = false;
                }

                evaluationState.AddProperty(propertyPath, isValid);
            }
        }

        return isValid;
    }
}