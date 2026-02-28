using System.Collections.Frozen;
using System.Text;

namespace Bebop.JsonSchema.Assertions.Object;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class DependentRequiredAssertion(FrozenDictionary<string, string[]> properties) : Assertion
{
    private readonly (string Name, byte[] Utf8Name, (string Name, byte[] Utf8Name)[] Dependents)[] _entries =
        properties.Select(kvp => (
            kvp.Key,
            Encoding.UTF8.GetBytes(kvp.Key),
            kvp.Value.Select(d => (d, Encoding.UTF8.GetBytes(d))).ToArray()
        )).ToArray();

    public override ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        JsonElement el = element.Element;
        if (el.ValueKind != JsonValueKind.Object)
        {
            // Non-objects shall be ignored
            return ValueTask.FromResult(true);
        }

        var isValid = true;
        foreach (var (_, utf8Name, dependents) in _entries)
        {
            if (!el.TryGetProperty(utf8Name, out _)) 
                continue;

            foreach (var (depName, depUtf8Name) in dependents)
            {
                if (el.TryGetProperty(depUtf8Name, out _)) 
                    continue;

                isValid = false;
                errorCollection.AddError($"Missing dependent property '{depName}'", element);
            }
        }

        return ValueTask.FromResult(isValid);
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"dependentRequired ({_entries.Length} properties)";
}
