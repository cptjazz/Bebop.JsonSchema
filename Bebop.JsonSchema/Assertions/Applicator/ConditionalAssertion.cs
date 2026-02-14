namespace Bebop.JsonSchema.Assertions.Applicator;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class ConditionalAssertion(JsonSchema ifSchema, JsonSchema thenSchema, JsonSchema elseSchema)
    : Assertion
{
    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        return await ifSchema.Validate(element, evaluationState, errorCollection).ConfigureAwait(false)
            ? await thenSchema.Validate(element, evaluationState, errorCollection).ConfigureAwait(false)
            : await elseSchema.Validate(element, evaluationState, errorCollection).ConfigureAwait(false);
    }

    public override async ValueTask PrepareImpl()
    {
        await SyncContext.Drop();

        await ifSchema.Prepare();
        await thenSchema.Prepare();
        await elseSchema.Prepare();
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => "if/then/else";
}
