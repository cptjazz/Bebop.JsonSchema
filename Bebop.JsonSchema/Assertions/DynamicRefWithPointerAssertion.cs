namespace Bebop.JsonSchema.Assertions;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Core)]
internal sealed class DynamicRefWithPointerAssertion(
    Uri schemaUri,
    SchemaRegistry repo,
    JsonPointer schemaPath)
    : Assertion
{
    public override string[] AssociatedKeyword => ["$dynamicRef"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        var schema = _LoadDynamicSubSchema(evaluationState);
        return schema.Validate(element, evaluationState, errorCollection);
    }

    private JsonSchema _LoadDynamicSubSchema(EvaluationState evaluationState)
    {
        if (repo.TryGetSchema(schemaUri, out var schema))
        {
            // A $dynamicRef to an $anchor in the same schema resource behaves like a normal $ref to an $anchor
            if (schema.Anchors.TryGetSchemaByNamedAnchor(schemaPath, out var inn))
            {
                return inn;
            }

            foreach (var s in evaluationState.SchemaStack)
            {
                if (s.Anchors.TryGetSchemaDynamicNamedAnchor(schemaPath, out var inner))
                {
                    return inner;
                }
            }

            if (schema.Anchors.TryGetSchemaDynamicNamedAnchor(schemaPath, out var innerSchema))
            {
                return innerSchema;
            }


            if (schema.Anchors.TryGetSchemaByAnonymousAnchor(schemaPath, out var innerSchema2))
            {
                return innerSchema2;
            }

            throw new InvalidSchemaException($"Referenced dynamicAnchor '{schemaPath}' not found.");
        }

        throw new InvalidSchemaException($"Referenced schema '{schemaUri}' not found.");
    }
}