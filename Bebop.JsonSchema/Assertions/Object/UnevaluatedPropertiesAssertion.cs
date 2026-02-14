namespace Bebop.JsonSchema.Assertions.Object;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class UnevaluatedPropertiesAssertion(JsonSchema schema) : Assertion
{
    public override int Order => 1000;

    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
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
                if (!await schema.Validate(h, evaluationState, errorCollection).ConfigureAwait(false))
                {
                    errorCollection.AddError($"Property '{property.Name}' does not match schema in 'unevaluatedProperties' assertion.", h);
                    isValid = false;
                }

                evaluationState.AddProperty(propertyPath, isValid);
            }
        }

        return isValid;
    }

    public override ValueTask PrepareImpl()
    {
        return schema.Prepare();
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => "unevaluatedProperties";
}
