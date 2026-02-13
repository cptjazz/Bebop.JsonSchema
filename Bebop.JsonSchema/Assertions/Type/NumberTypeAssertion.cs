using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Bebop.JsonSchema.Assertions.Type;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class NumberTypeAssertion : TypeAssertion
{
    public static readonly NumberTypeAssertion Instance = new();
    public override ValueTask<bool> Assert(in Token element, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind == JsonValueKind.Number) 
            return ValueTask.FromResult(true);
        
        return ValueTask.FromResult(_AddError(errorCollection, element));

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool _AddError(ErrorCollection ec, in Token e)
        {
            ec.AddError("Element is not a number.", e);
            return false;
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => "type = number";
}