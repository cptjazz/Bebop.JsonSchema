namespace Bebop.JsonSchema.Assertions.Applicator;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class OneOfAssertion(JsonSchema[] schemas) : Assertion
{
    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        var c = 0;
        EvaluationState fes = evaluationState;
        foreach (var schema in schemas)
        {
            var es = evaluationState.New();
            if (await schema.Validate(element, es, NopErrorCollection.Instance).ConfigureAwait(false))
            {
                c++;
                if (c == 1)
                {
                    fes = es;
                }
            }
        }

        switch (c)
        {
            case > 1:
                errorCollection.AddError("Element matches multiple schemas in 'oneOf' assertion.", element);
                return false;
            case 0:
                errorCollection.AddError("Element matches no schema in 'oneOf' assertion.", element);
                return false;
            default:
                evaluationState.Absorb(fes);
                return true;
        }
    }

    public override async ValueTask PrepareImpl()
    {
        await SyncContext.Drop();

        foreach (var schema in schemas)
        {
            await schema.Prepare();
        }
    }
}