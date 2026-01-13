namespace Bebop.JsonSchema.Assertions.Object;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Unevaluated)]
internal sealed class UnevaluatedItemsAssertion(JsonSchema schema) : Assertion
{
    public override string[] AssociatedKeyword => ["unevaluatedItems"];

    public override int Order => 1000;

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind != JsonValueKind.Array)
            return true;

        var isValid = true;
        var i = 0;

        foreach (var e in element.Element.EnumerateArray())
        {
            var propertyPath = element.ElementPath.AppendIndex(i);
            
            if (evaluationState.IsPropertyUnevaluated(propertyPath))
            {
                var h = new Token(in e, propertyPath);
                if (!schema.Validate(h, evaluationState, errorCollection))
                {
                    errorCollection.AddError("Array element does not match schema in 'unevaluatedItems' assertion.", h);
                    isValid = false;
                }

                evaluationState.AddProperty(propertyPath, isValid);
            }

            i++;
        }

        return isValid;
    }
}