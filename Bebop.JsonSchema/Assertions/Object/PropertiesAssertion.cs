using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Object;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class PropertiesAssertion(FrozenDictionary<string, JsonSchema> properties) : Assertion
{
    public override string[] AssociatedKeyword => ["properties"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        JsonElement el = element.Element;
        if (el.ValueKind != JsonValueKind.Object)
        {
            // Non-objects shall be ignored
            return true;
        }

        var isValid = true;

        foreach (var (name, schema) in properties)
        {
            if (el.TryGetProperty(name, out var property))
            {
                var propertyPath = element.ElementPath.AppendPropertyName(name);
                
                var h = new Token(in property, propertyPath);
                if (!schema.Validate(h, evaluationState, errorCollection))
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

    public override async ValueTask Prepare()
    {
        foreach (var (_, s) in properties)
        {
            await s
                .Prepare()
                .ConfigureAwait(false);
        }
    }
}