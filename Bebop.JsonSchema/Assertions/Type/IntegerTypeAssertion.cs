using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Type;

internal sealed class IntegerTypeAssertion : TypeAssertion
{
    public static readonly IntegerTypeAssertion Instance = new();
    public override bool Assert(in Token element, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind != JsonValueKind.Number)
            return false;

        if (element.Element.TryGetInt64(out _)) 
            return true;

        if (element.Element.TryGetDouble(out double doubleValue) && doubleValue % 1 == 0)
            return true;

        return _AddError(errorCollection, element);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool _AddError(ErrorCollection ec, in Token e)
        {
            ec.AddError("Element is not an integer.", e);
            return false;
        }
    }
}