using System.Collections.Frozen;

namespace Bebop.JsonSchema.Assertions.Object;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class DependentRequiredAssertion(FrozenDictionary<string, string[]> properties) : Assertion
{
    public override ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        JsonElement el = element.Element;
        if (el.ValueKind != JsonValueKind.Object)
        {
            // Non-objects shall be ignored
            return ValueTask.FromResult(true);
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

        return ValueTask.FromResult(isValid);
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"dependentRequired ({properties.Count} properties)";
}
