using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Number;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class MultipleOfAssertion(double multipleOf) : NumberAssertion
{
    public override string[] AssociatedKeyword => ["multipleOf"];

    protected override bool Assert(double value, in Token element, ErrorCollection errorCollection)
    {
        var quotient = value / multipleOf;

        if (double.IsInfinity(quotient))
        {
            return _DecimalFallback(value, element, errorCollection);
        }

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

        bool _DecimalFallback(double value, Token element, ErrorCollection errorCollection)
        {
            // Use decimal arithmetic when double overflows
            try
            {
                var decValue = (decimal)value;
                var decMultipleOf = (decimal)multipleOf;
                if (decMultipleOf != 0m && decValue % decMultipleOf == 0m)
                    return true;
            }
            catch (OverflowException)
            {
                // If decimal can't hold the value either, fall through to error
            }

            return _AddError(errorCollection, element);
        }
    }
}