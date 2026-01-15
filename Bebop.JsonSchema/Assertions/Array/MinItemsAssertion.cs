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
        
        return _AddError(errorCollection, array);

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool _AddError(ErrorCollection ec, in Token e)
        {
            ec.AddError($"Array must have at least '{minItems}' items", e);
            return false;
        }
    }
}