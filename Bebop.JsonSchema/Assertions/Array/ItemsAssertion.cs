namespace Bebop.JsonSchema.Assertions.Array;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class ItemsAssertion(JsonSchema schema) : Assertion
{
    public JsonSchema Schema { get; } = schema;

    public override string[] AssociatedKeyword => ["items"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        JsonElement el = element.Element;
        if (el.ValueKind != JsonValueKind.Array)
            return true;

        var isValid = true;
        var i = 0;
        var l = el.GetArrayLength();

        using var absorptionList = Pool.RentArray<EvaluationState>(l);

        foreach (var e in el.EnumerateArray())
        {
            var es = evaluationState.New();
            if (!Schema.Validate(element.Subitem(in e, i), es, errorCollection))
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
}