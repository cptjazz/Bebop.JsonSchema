using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Number;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class MultipleOfAssertion(double multipleOf) : NumberAssertion
{
    public override string[] AssociatedKeyword => ["multipleOf"];

    protected override bool Assert(double value, in Token element, ErrorCollection errorCollection)
    {
        var quotient = value / multipleOf;
        var remainder = Math.Abs(quotient - Math.Round(quotient));
        
        // Use epsilon tolerance for floating-point comparison
        const double epsilon = 1e-15;
        
        if (remainder < epsilon)
        {
            return true; // Value IS a multiple - assertion passes
        }

        return _AddError(errorCollection, element);

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool _AddError(ErrorCollection ec, Token e)
        {
            ec.AddError($"Value is not a multiple of {multipleOf}.", e);
            return false;
        }
    }
}