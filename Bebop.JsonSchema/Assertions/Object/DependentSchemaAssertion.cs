namespace Bebop.JsonSchema.Assertions.Object;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class DependentSchemaAssertion(Dictionary<string, JsonSchema> properties) : Assertion
{
    public override string[] AssociatedKeyword { get; } = ["dependentSchemas"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind != JsonValueKind.Object) 
            return true;

        var isValid = true;

        foreach (var (name, schema) in properties)
        {
            if (element.Element.TryGetProperty(name, out _))
            {
                if (!schema.Validate(element, evaluationState, errorCollection))
                {
                    errorCollection.AddError($"Element did not match dependent schema '{name}'.", element);
                    isValid = false;
                }
            }
        }

        return isValid;
    }
}