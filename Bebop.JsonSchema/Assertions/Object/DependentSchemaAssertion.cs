using System.Collections.Frozen;

namespace Bebop.JsonSchema.Assertions.Object;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class DependentSchemaAssertion(FrozenDictionary<string, JsonSchema> properties) : Assertion
{
    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind != JsonValueKind.Object) 
            return true;

        var isValid = true;

        foreach (var (name, schema) in properties)
        {
            if (element.Element.TryGetProperty(name, out _))
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

        foreach (var (_, schema) in properties)
        {
            await schema.Prepare();
        }
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"dependentSchemas ({properties.Count} schemas)";
}
