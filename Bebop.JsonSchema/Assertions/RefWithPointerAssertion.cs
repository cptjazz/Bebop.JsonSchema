namespace Bebop.JsonSchema.Assertions;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class RefWithPointerAssertion(
    Uri schemaUri,
    SchemaRegistry repo,
    JsonPointer schemaPath)
    : Assertion
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

    public override async ValueTask PrepareImpl()
    {
        await SyncContext.Drop();

        _schema ??= await _LoadSubSchema();
        await _schema.Prepare();
    }

    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"$ref → {schemaUri}#{schemaPath}";
}
