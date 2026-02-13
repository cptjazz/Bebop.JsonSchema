using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Bebop.JsonSchema.Assertions.String;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class PatternAssertion(string pattern) : StringAssertion
{
    private readonly Regex _regex = new(pattern, RegexOptions.Compiled | RegexOptions.ECMAScript);

    public override bool AssertProperty(string text, in Token element, ErrorCollection errorCollection)
    {
        if (_regex.IsMatch(text))
            return true;

        return _AddError(errorCollection, element);

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool _AddError(ErrorCollection ec, in Token e)
        {
            ec.AddError($"String does not match the required pattern: {pattern}.", e);
            return false;
        }
    }
}