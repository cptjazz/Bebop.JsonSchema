namespace Bebop.JsonSchema.Assertions;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Core)]
internal sealed class RefWithPointerAssertion(
    Uri schemaUri,
    SchemaRegistry repo,
    JsonPointer schemaPath)
    : Assertion
{
    private JsonSchema? _schema;

    public override string[] AssociatedKeyword => ["$ref"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        _schema ??= _LoadStaticSubSchema();

        return _schema.Validate(element, evaluationState, errorCollection);
    }

    private JsonSchema _LoadStaticSubSchema()
    {
        if (repo.TryGetSchema(schemaUri, out var schema))
        {
            if (schema.Anchors.TryGetSchemaByNamedAnchor(schemaPath, out var innerSchema))
                return innerSchema;

            // A $ref to a $dynamicAnchor in the same schema resource behaves like a normal $ref to an $anchor
            if (schema.Anchors.TryGetSchemaDynamicNamedAnchor(schemaPath, out var innerSchema2))
                return innerSchema2;

            if (schema.Anchors.TryGetSchemaByAnonymousAnchor(schemaPath, out var innerSchema3))
                return innerSchema3;

            throw new InvalidSchemaException($"Referenced anchor '{schemaPath.ToStringWithoutEncoding()}' not found.");
        }

        throw new InvalidSchemaException($"Referenced schema '{schemaUri}' not found.");
    }

    public override ValueTask Prepare()
    {
        _schema ??= _LoadStaticSubSchema();
        return _schema.RootAssertion.Prepare();
    }
}