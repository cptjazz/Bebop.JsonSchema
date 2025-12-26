using System.Diagnostics;

namespace Bebop.JsonSchema;

internal abstract class Assertion
{
    public abstract ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection);

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual int Order => 0;

    public virtual async ValueTask Prepare()
    {
        if (IsPrepared)
            return;

        await PrepareImpl().ConfigureAwait(false);
        IsPrepared = true;
    }

    public virtual ValueTask PrepareImpl() => ValueTask.CompletedTask;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    protected bool IsPrepared { get; set; }

    protected void EnsurePrepared()
    {
        if (!IsPrepared)
            throw new InvalidOperationException("Assertion not prepared. Call Prepare() before using the assertion.");
    }
}
