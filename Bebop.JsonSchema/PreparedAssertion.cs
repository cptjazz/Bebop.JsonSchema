namespace Bebop.JsonSchema;

internal abstract class PreparedAssertion : Assertion
{
    protected PreparedAssertion()
    {
        IsPrepared = true;
    }

    public sealed override ValueTask Prepare() => ValueTask.CompletedTask;
}
