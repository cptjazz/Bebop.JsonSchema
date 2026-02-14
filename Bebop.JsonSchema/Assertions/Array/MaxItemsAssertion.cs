using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Array;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class MaxItemsAssertion(int maxItems) : ArrayAssertion
{
    public override bool Assert(in Token array, ErrorCollection errorCollection)
    {
        var count = array.Element.GetArrayLength();

        if (!(count > maxItems)) 
            return true;
        
        return _AddError(errorCollection, array);

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool _AddError(ErrorCollection ec, in Token e)
        {
            ec.AddError($"Array must have at most '{maxItems}' items", e);
            return false;
        }
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"count(items) <= {maxItems}";
}
