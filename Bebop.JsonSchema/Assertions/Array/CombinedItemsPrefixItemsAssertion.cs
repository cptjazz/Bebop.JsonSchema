namespace Bebop.JsonSchema.Assertions.Array;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class CombinedItemsPrefixItemsAssertion(JsonSchema[] prefixItemsSchemas, JsonSchema itemsSchema) : Assertion
{
    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind != JsonValueKind.Array)
            return true;

        var i = 0;
        var isValid = true;
        using var absorptionList = Pool.RentArray<EvaluationState>(element.Element.GetArrayLength());

        foreach (var e in element.Element.EnumerateArray())
        {
            var itemPath = element.ElementPath.AppendIndex(i);

            var h = new Token(in e, itemPath);

            if (i >= prefixItemsSchemas.Length)
            {
                var es = evaluationState.New();
                if (!await itemsSchema.Validate(h, es, errorCollection).ConfigureAwait(false))
                {
                    isValid = false;
                }
                else
                {
                    absorptionList.Add(es);
                }

                evaluationState.AddProperty(element.ElementPath.AppendIndex(i), isValid);
                i++;
                continue;
            }

            if (!await prefixItemsSchemas[i].Validate(h, evaluationState, errorCollection).ConfigureAwait(false))
                isValid = false;

            evaluationState.AddProperty(element.ElementPath.AppendIndex(i), isValid);
            i++;
        }

        foreach (var e in absorptionList.AsSpan())
        {
            evaluationState.Absorb(e);
        }

        return isValid;
    }

    public override async ValueTask PrepareImpl()
    {
        await SyncContext.Drop();

        await itemsSchema.Prepare();
        foreach (var schema in prefixItemsSchemas)
        {
            await schema.Prepare();
        }
    }
}