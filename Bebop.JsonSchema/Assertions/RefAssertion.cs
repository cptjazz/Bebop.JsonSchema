namespace Bebop.JsonSchema.Assertions;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Core)]
internal sealed class RefAssertion(Uri schemaUri, SchemaRegistry repo) : Assertion
{
    private JsonSchema? _schema;

    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        EnsurePrepared();
        return await _schema!.Validate(element, evaluationState, errorCollection).ConfigureAwait(false);
    }

    private async ValueTask<JsonSchema> _LoadSubSchema()
    {
        var schema = await repo
            .GetSchema(schemaUri)
            .ConfigureAwait(false);

        if (schema is not null)
        {
            return schema;
        }

        throw new InvalidSchemaException($"Referenced schema '{schemaUri}' not found.");
    }

    public override async ValueTask PrepareImpl()
    {
        await SyncContext.Drop();
       
        _schema ??= await _LoadSubSchema();
        await _schema.Prepare();
    }
}