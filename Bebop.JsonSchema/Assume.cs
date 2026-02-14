using System.Diagnostics.CodeAnalysis;

namespace Bebop.JsonSchema;

internal static class Assume
{
    public static Assumption That([DoesNotReturnIf(false)] bool condition)
    {
        return condition
            ? TrueAssumption.Instance 
            : FalseAssumption.Instance;
    }

    internal abstract class Assumption
    {
        public abstract void OtherwiseThrow(Func<Exception> exceptionFactory);
    }

    internal sealed class TrueAssumption : Assumption
    {
        public static readonly TrueAssumption Instance = new();

        public override void OtherwiseThrow(Func<Exception> exceptionFactory)
        {
        }
    }

    internal sealed class FalseAssumption : Assumption
    {
        public static readonly FalseAssumption Instance = new();
        [DoesNotReturn]
        public override void OtherwiseThrow(Func<Exception> exceptionFactory)
        {
            throw exceptionFactory();
        }
    }
}
