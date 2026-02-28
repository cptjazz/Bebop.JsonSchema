using System.Runtime.CompilerServices;
using System.Text;

namespace Bebop.JsonSchema.Assertions.Object;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class RequiredAssertion(string[] requiredProperties) : Assertion
{
    private readonly (string Name, byte[] Utf8Name)[] _entries =
        requiredProperties.Select(n => (n, Encoding.UTF8.GetBytes(n))).ToArray();

    public override ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        JsonElement el = element.Element;
        if (el.ValueKind != JsonValueKind.Object)
        {
            // Non-objects shall be ignored
            return ValueTask.FromResult(true);
        }

        bool isValid = true;

        foreach (var (name, utf8Name) in _entries)
        {
            if (!el.TryGetProperty(utf8Name, out _))
            {
                _AddError(element, errorCollection, name);
                isValid = false;
            }
        }

        return ValueTask.FromResult(isValid);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void _AddError(in Token e, ErrorCollection ec, string p)
        {
            ec.AddError($"Object is missing required property '{p}'", e);
        }
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"required ({_entries.Length} properties)";
}
