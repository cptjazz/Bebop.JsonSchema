namespace Bebop.JsonSchema.Assertions;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Core)]
internal sealed class DynamicRefWithPointerAssertion(
    Uri schemaUri,
    SchemaRegistry repo,
    JsonPointer schemaPath)
    : Assertion
{
    public override string[] AssociatedKeyword => ["$dynamicRef"];

    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        var schema = await _LoadDynamicSubSchema(evaluationState).ConfigureAwait(false);
        return await schema.Validate(element, evaluationState, errorCollection).ConfigureAwait(false);
    }

    private async ValueTask<JsonSchema> _LoadDynamicSubSchema(EvaluationState evaluationState)
    {
        var schema = await repo
            .GetSchema(schemaUri)
            .ConfigureAwait(false);

        if (schema is not null)
        {
            await SyncContext.Drop();

            // A $dynamicRef to an $anchor in the same schema resource behaves like a normal $ref to an $anchor
            if (schema.Anchors.TryGetSchemaByNamedAnchor(schemaPath, out var inn))
            {
                await inn.Prepare();
                return inn;
            }

            foreach (var s in evaluationState.SchemaStack)
            {
                if (s.Anchors.TryGetSchemaDynamicNamedAnchor(schemaPath, out var inner))
                {
                    await inner.Prepare();
                    return inner;
                }
            }

            if (schema.Anchors.TryGetSchemaDynamicNamedAnchor(schemaPath, out var innerSchema))
            {
                await innerSchema.Prepare();
                return innerSchema;
            }


            if (schema.Anchors.TryGetSchemaByAnonymousAnchor(schemaPath, out var innerSchema2))
            {
                await innerSchema2.Prepare();
                return innerSchema2;
            }

            throw new InvalidSchemaException($"Referenced dynamicAnchor '{schemaPath}' not found.");
        }

        throw new InvalidSchemaException($"Referenced schema '{schemaUri}' not found.");
    }
}