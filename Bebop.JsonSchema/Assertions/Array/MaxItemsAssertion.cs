using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Array;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class MaxItemsAssertion(int maxItems) : ArrayAssertion
{
    public override string[] AssociatedKeyword => ["maxItems"];

    public override bool Assert(in Token array, ErrorCollection errorCollection)
    {
        var count = array.Element.GetArrayLength();

        if (!(count > maxItems)) 
            return true;
        
        return _AddError(errorCollection, array, maxItems);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool _AddError(ErrorCollection ec, in Token e, int m)
        {
            ec.AddError($"Array must have at most '{m}' items", e);
            return false;
        }
    }
}