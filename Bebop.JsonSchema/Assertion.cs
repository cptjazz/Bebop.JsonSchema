using System.Diagnostics;

namespace Bebop.JsonSchema;

internal abstract class Assertion
{
    public abstract bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection);

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual int Order => 0;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public abstract string[] AssociatedKeyword { get; }

    public virtual ValueTask Prepare() => ValueTask.CompletedTask;
}