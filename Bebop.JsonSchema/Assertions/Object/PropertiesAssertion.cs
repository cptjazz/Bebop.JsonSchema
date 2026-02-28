using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using System.Text;

namespace Bebop.JsonSchema.Assertions.Object;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class PropertiesAssertion(FrozenDictionary<string, JsonSchema> properties) : Assertion
{
    private readonly (string Name, byte[] Utf8Name, JsonSchema Schema)[] _entries =
        properties.Select(kvp => (kvp.Key, Encoding.UTF8.GetBytes(kvp.Key), kvp.Value)).ToArray();

    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        JsonElement el = element.Element;
        if (el.ValueKind != JsonValueKind.Object)
        {
            // Non-objects shall be ignored
            return true;
        }

        var isValid = true;

        foreach (var (name, utf8Name, schema) in _entries)
        {
            if (el.TryGetProperty(utf8Name, out var property))
            {
                var propertyPath = element.ElementPath.AppendPropertyName(name);

                var h = new Token(in property, propertyPath);
                if (!await schema.Validate(h, evaluationState, errorCollection).ConfigureAwait(false))
                {
                    isValid = false;
                    _AddError(errorCollection, name, h);
                }

                evaluationState.AddProperty(propertyPath, isValid);
            }
        }

        return isValid;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void _AddError(ErrorCollection ec, string name, in Token h)
        {
            ec.AddError($"Property does not match the schema defined for '{name}'.",
                        h);
        }
    }

    public override async ValueTask PrepareImpl()
    {
        foreach (var (_, _, schema) in _entries)
        {
            await schema
                .Prepare()
                .ConfigureAwait(false);
        }
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"properties ({_entries.Length} properties)";
}
