using System.Globalization;
using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.String;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class MaxLengthAssertion(int maxLength) : StringAssertion
{
    public override bool AssertProperty(string text, in Token element, ErrorCollection errorCollection)
    {
        var si = new StringInfo(text);
        if (si.LengthInTextElements <= maxLength)
            return true;

        return _AddError(errorCollection, element, maxLength);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool _AddError(ErrorCollection ec, in Token e, int m)
        {
            ec.AddError($"String length exceeds maximum length of {m}.", e);
            return false;
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"len(string) <= {maxLength}";
}