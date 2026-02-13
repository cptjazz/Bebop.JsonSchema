using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Type;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class BooleanTypeAssertion : TypeAssertion
{
    public static readonly BooleanTypeAssertion Instance = new();

    public override ValueTask<bool> Assert(in Token element, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind is JsonValueKind.True or JsonValueKind.False) 
            return ValueTask.FromResult(true);

        return ValueTask.FromResult(_AddError(errorCollection, element));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool _AddError(ErrorCollection ec, in Token e)
        {
            ec.AddError("Element is not a boolean.", e);
            return false;
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => "type = boolean";
}