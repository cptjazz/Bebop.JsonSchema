using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Type;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class IntegerTypeAssertion : TypeAssertion
{
    public static readonly IntegerTypeAssertion Instance = new();
    public override ValueTask<bool> Assert(in Token element, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind != JsonValueKind.Number)
            return ValueTask.FromResult(false);

        if (element.Element.TryGetInt64(out _)) 
            return ValueTask.FromResult(true);

        if (element.Element.TryGetDouble(out double doubleValue) && doubleValue % 1 == 0)
            return ValueTask.FromResult(true);

        return ValueTask.FromResult(_AddError(errorCollection, element));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool _AddError(ErrorCollection ec, in Token e)
        {
            ec.AddError("Element is not an integer.", e);
            return false;
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => "type = integer";
}