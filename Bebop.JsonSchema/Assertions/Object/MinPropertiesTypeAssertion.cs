using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Object;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class MinPropertiesAssertion(int minProperties) : Assertion
{
    public override ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        JsonElement el = element.Element;
        if (el.ValueKind != JsonValueKind.Object)
            return ValueTask.FromResult(true);

        if (el.GetPropertyCount() < minProperties)
        {
            _AddError(element, errorCollection, minProperties);
            return ValueTask.FromResult(false);
        }

        return ValueTask.FromResult(true);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void _AddError(in Token e, ErrorCollection ec, int m)
        {
            ec.AddError($"Object must have at least {m} properties", e);
        }
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"count(properties) >= {minProperties}";
}
