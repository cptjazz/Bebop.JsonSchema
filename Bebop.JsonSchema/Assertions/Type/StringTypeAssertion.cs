using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Type;

internal sealed class StringTypeAssertion : TypeAssertion
{
    public static readonly StringTypeAssertion Instance = new();

    public override bool Assert(in Token element, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind == JsonValueKind.String) 
            return true;

        return _AddError(errorCollection, element);
            
        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool _AddError(ErrorCollection ec, in Token e)
        {
            ec.AddError("Element is not a string.", e);
            return false;
        }
    }
}