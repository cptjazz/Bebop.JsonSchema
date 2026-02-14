using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Number;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class MinimumPropertyAssertion(double minimum) : NumberAssertion
{
    protected override bool Assert(double value, in Token element, ErrorCollection errorCollection)
    {
        if (value >= minimum) 
            return true;

        return _AddError(errorCollection, element);

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool _AddError(ErrorCollection ec, Token e)
        {
            ec.AddError($"Value is less than expected minimum '{minimum}'.", e);
            return false;
        }
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"x >= {minimum}";
}
