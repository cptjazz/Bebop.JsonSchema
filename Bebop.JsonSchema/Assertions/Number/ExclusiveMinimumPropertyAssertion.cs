using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Number;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class ExclusiveMinimumPropertyAssertion(double exclusiveMinimum) : NumberAssertion
{
    protected override bool Assert(double value, in Token element, ErrorCollection errorCollection)
    {
        if (!(value <= exclusiveMinimum)) 
            return true;

        return _AddError(errorCollection, element);

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool _AddError(ErrorCollection ec, Token e)
        {
            ec.AddError($"Value is not greater than expected exclusive minimum '{exclusiveMinimum}'.", e);
            return false;
        }
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"x > {exclusiveMinimum}";
}
