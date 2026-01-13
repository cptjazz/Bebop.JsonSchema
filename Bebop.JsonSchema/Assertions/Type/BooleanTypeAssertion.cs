using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Type;

internal sealed class BooleanTypeAssertion : TypeAssertion
{
    public static readonly BooleanTypeAssertion Instance = new();

    public override bool Assert(in Token element, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind is JsonValueKind.True or JsonValueKind.False) 
            return true;

        return _AddError(errorCollection, element);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool _AddError(ErrorCollection ec, in Token e)
        {
            ec.AddError("Element is not a boolean.", e);
            return false;
        }
    }
}