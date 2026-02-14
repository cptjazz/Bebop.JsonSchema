using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Array;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class UniqueItemsAssertion : ArrayAssertion
{
    public static readonly UniqueItemsAssertion Instance = new();

    public override bool Assert(in Token array, ErrorCollection errorCollection)
    {
        var seenItems = new HashSet<JsonElement>(JsonComparer.Instance);

        var i = 0;
        foreach (var item in array.Element.EnumerateArray())
        {
            if (seenItems.Add(item))
            {
                i++;
                continue;
            }

            return _AddError(errorCollection, array.Subitem(in item, i + 1));
        }

        return true;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool _AddError(ErrorCollection ec, in Token e)
        {
            ec.AddError("Array items must be unique", e);
            return false;
        }
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => "uniqueItems";
}
