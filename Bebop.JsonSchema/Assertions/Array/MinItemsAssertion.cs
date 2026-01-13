using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Array;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class MinItemsAssertion(int minItems) : ArrayAssertion
{
    public override string[] AssociatedKeyword => ["minItems"];

    public override bool Assert(in Token array, ErrorCollection errorCollection)
    {
        var count = array.Element.GetArrayLength();

        if (!(count < minItems)) 
            return true;
        
        return _AddError(errorCollection, array, minItems);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool _AddError(ErrorCollection ec, in Token e, int m)
        {
            ec.AddError($"Array must have at least '{m}' items", e);
            return false;
        }
    }
}