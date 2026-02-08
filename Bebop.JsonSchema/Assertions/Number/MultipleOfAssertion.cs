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
            // When quotient overflows to infinity, the value is extremely large.
            // If the value is an integer (no fractional part) and multipleOf
            // evenly divides 1.0, then multipleOf divides all integers.
            if (value == Math.Truncate(value))
            {
                var invRemainder = 1.0 % multipleOf;
                if (Math.Abs(invRemainder) < 1e-15 || Math.Abs(invRemainder - multipleOf) < 1e-15)
                    return true;
            }

            // Try decimal arithmetic as a fallback
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