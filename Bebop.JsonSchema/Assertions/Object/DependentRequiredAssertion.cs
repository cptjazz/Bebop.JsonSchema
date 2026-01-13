using System.Collections.Frozen;

namespace Bebop.JsonSchema.Assertions.Object;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class DependentRequiredAssertion(FrozenDictionary<string, string[]> properties) : Assertion
{
    public override string[] AssociatedKeyword => ["dependentRequired"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        JsonElement el = element.Element;
        if (el.ValueKind != JsonValueKind.Object)
        {
            // Non-objects shall be ignored
            return true;
        }

        var isValid = true;
        foreach (var property in properties)
        {
            if (!el.TryGetProperty(property.Key, out _)) 
                continue;

            foreach (var dependent in property.Value)
            {
                if (el.TryGetProperty(dependent, out _)) 
                    continue;

                isValid = false;
                errorCollection.AddError($"Missing dependent property '{dependent}'", element);
            }
        }

        return isValid;
    }
}