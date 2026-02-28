using System.Globalization;
using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.String;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class MinMaxLengthAssertion(int minLength, int maxLength) : StringAssertion
{
    public override bool AssertProperty(string text, in Token element, ErrorCollection errorCollection)
    {
        var si = new StringInfo(text);
        int l = si.LengthInTextElements;
        if (l <= maxLength && l >= minLength)
            return true;

        return _AddError(errorCollection, element, minLength, maxLength);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool _AddError(ErrorCollection ec, in Token e, int mi, int ma)
        {
            ec.AddError($"String length is not between {mi} and {ma}.", e);
            return false;
        }
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{minLength} <= len(string) <= {maxLength}";
}
