namespace Bebop.JsonSchema.Assertions.Array;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class AdditionalItemsAssertion(JsonSchema schema) : Assertion
{
    public JsonSchema Schema { get; } = schema;

    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
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
            if (!await Schema.Validate(element.Subitem(in e, i), es, errorCollection).ConfigureAwait(false))
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

    public override ValueTask PrepareImpl()
    {
        return schema.Prepare();
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => "additionalItems";
}

