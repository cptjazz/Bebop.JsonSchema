namespace Bebop.JsonSchema.Assertions;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Core)]
internal sealed class RefAssertion(Uri schemaUri, SchemaRegistry repo) : Assertion
{
    private JsonSchema? _schema;

    public override string[] AssociatedKeyword => ["$ref"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        _schema ??= _LoadSubSchema();

        return _schema.Validate(element, evaluationState, errorCollection);
    }

    private JsonSchema _LoadSubSchema()
    {
        if (repo.TryGetSchema(schemaUri, out var schema))
        {
            return schema;
        }

        throw new InvalidSchemaException($"Referenced schema '{schemaUri}' not found.");
    }

    public override ValueTask Prepare()
    {
        _schema ??= _LoadSubSchema();
        return _schema.RootAssertion.Prepare();
    }
}