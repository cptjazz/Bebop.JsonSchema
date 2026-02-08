using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace Bebop.JsonSchema.Assertions.Object;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class PatternPropertiesAssertion(FrozenDictionary<string, JsonSchema> properties) : Assertion
{
    private readonly FrozenDictionary<string, Regex> _regexes = properties
        .ToFrozenDictionary(
            x => x.Key, 
            x => new Regex(x.Key, RegexOptions.Compiled | RegexOptions.ECMAScript));

    public override string[] AssociatedKeyword => ["patternProperties"];

    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        JsonElement el = element.Element;
        if (el.ValueKind != JsonValueKind.Object)
        {
            // Non-objects shall be ignored
            return true;
        }

        var isValid = true;

        foreach (var (name, regex) in _regexes)
        {
            foreach (var e in el.EnumerateObject())
            {
                if (!regex.IsMatch(e.Name))
                    continue;

                var schema = properties[name];
                var propertyPath = element.ElementPath.AppendPropertyName(e.Name);

                var value = e.Value;
                var h = new Token(in value, propertyPath);
                if (!await schema.Validate(h, evaluationState, errorCollection).ConfigureAwait(false))
                {
                    isValid = false;
                    _AddError(errorCollection, name, e, h);
                }

                evaluationState.AddProperty(propertyPath, isValid);
            }
        }

        return isValid;

        static void _AddError(ErrorCollection ec, string name, in JsonProperty e, in Token h)
        {
            ec.AddError($"Property '{e.Name}' does not match the schema defined by pattern '{name}'.",
                        h);
        }
    }

    public override async ValueTask PrepareImpl()
    {
        await SyncContext.Drop();

        foreach (var (_, schema) in properties)
        {
            await schema.Prepare();
        }
    }
}