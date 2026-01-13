namespace Bebop.JsonSchema.Assertions.Array;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class CombinedItemsPrefixItemsAssertion(JsonSchema[] prefixItemsSchemas, JsonSchema itemsSchema) : Assertion
{
    public override string[] AssociatedKeyword => ["items", "prefixItems"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
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
                if (!itemsSchema.Validate(h, es, errorCollection))
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

            if (!prefixItemsSchemas[i].Validate(h, evaluationState, errorCollection))
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
}