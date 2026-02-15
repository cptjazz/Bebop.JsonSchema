using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Bebop.JsonSchema.Assertions.String;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class PatternAssertion : StringAssertion
{
    private readonly Regex _regex;
    private readonly string _pattern;


    public PatternAssertion(string pattern)
    {
        _pattern = RegexNormalizer.Normalize(pattern);
        _regex = new(_pattern, RegexOptions.Compiled | RegexOptions.ECMAScript);
    }

    public override bool AssertProperty(string text, in Token element, ErrorCollection errorCollection)
    {
        if (_regex.IsMatch(text))
            return true;

        return _AddError(errorCollection, element);

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool _AddError(ErrorCollection ec, in Token e)
        {
            ec.AddError($"String does not match the required pattern: {_pattern}.", e);
            return false;
        }
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"pattern = {_pattern}";
}
