namespace Bebop.JsonSchema.Assertions.Array;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class PrefixItemsAssertion(JsonSchema[] schemas) : Assertion
{
    public override string[] AssociatedKeyword => ["prefixItems"];

    public JsonSchema[] Schemas { get; } = schemas;

    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind != JsonValueKind.Array)
            return true;

        var i = 0;
        var isValid = true;
        using var absorptionList = Pool.RentArray<EvaluationState>(element.Element.GetArrayLength());

        foreach (var e in element.Element.EnumerateArray())
        {
            if (i >= Schemas.Length)
                return isValid;

            var es = evaluationState.New();
            if (!await Schemas[i].Validate(element.Subitem(in e, i), es, errorCollection).ConfigureAwait(false))
            {
                isValid = false;
            }
            else
            {
                absorptionList.Add(es);
            }

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

        foreach (var schema in schemas)
        {
            await schema.Prepare();
        }
    }
}