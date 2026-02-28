using System.Collections.Frozen;
using System.Text;

namespace Bebop.JsonSchema.Assertions.Object;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class DependentSchemaAssertion(FrozenDictionary<string, JsonSchema> properties) : Assertion
{
    private readonly (string Name, byte[] Utf8Name, JsonSchema Schema)[] _entries =
        properties.Select(kvp => (kvp.Key, Encoding.UTF8.GetBytes(kvp.Key), kvp.Value)).ToArray();

    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind != JsonValueKind.Object) 
            return true;

        var isValid = true;

        foreach (var (name, utf8Name, schema) in _entries)
        {
            if (element.Element.TryGetProperty(utf8Name, out _))
            {
                if (!await schema.Validate(element, evaluationState, errorCollection).ConfigureAwait(false))
                {
                    errorCollection.AddError($"Element did not match dependent schema '{name}'.", element);
                    isValid = false;
                }
            }
        }

        return isValid;
    }

    public override async ValueTask PrepareImpl()
    {
        await SyncContext.Drop();

        foreach (var (_, _, schema) in _entries)
        {
            await schema.Prepare();
        }
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"dependentSchemas ({_entries.Length} schemas)";
}
