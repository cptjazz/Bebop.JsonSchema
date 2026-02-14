using System.Globalization;
using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.String;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class MinLengthAssertion(int minLength) : StringAssertion
{
    public override bool AssertProperty(string text, in Token element, ErrorCollection errorCollection)
    {
        var si = new StringInfo(text);
        if (si.LengthInTextElements >= minLength)
            return true;

        return _AddError(errorCollection, element, minLength);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool _AddError(ErrorCollection ec, in Token e, int m)
        {
            ec.AddError($"String length is less than minimum length of {m}.", e);
            return false;
        }
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"len(string) >= {minLength}";
}
