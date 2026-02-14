using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Number;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class MaximumPropertyAssertion(double maximum) : NumberAssertion
{
    protected override bool Assert(double value, in Token element, ErrorCollection errorCollection)
    {
        if (value <= maximum) 
            return true;

        return _AddError(errorCollection, element);

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool _AddError(ErrorCollection ec, Token e)
        {
            ec.AddError($"Value is greater than expected maximum '{maximum}'.", e);
            return false;
        }
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"x <= {maximum}";
}
