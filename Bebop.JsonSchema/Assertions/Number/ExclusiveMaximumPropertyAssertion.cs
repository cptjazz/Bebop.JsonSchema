using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Number;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class ExclusiveMaximumPropertyAssertion(double exclusiveMaximum) : NumberAssertion
{
    protected override bool Assert(double value, in Token element, ErrorCollection errorCollection)
    {
        if (!(value >= exclusiveMaximum)) 
            return true;

        return _AddError(errorCollection, element);

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool _AddError(ErrorCollection ec, in Token e)
        {
            ec.AddError($"Value is not less than expected exclusive maximum '{exclusiveMaximum}'.", e);
            return false;
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"x < {exclusiveMaximum}";
}