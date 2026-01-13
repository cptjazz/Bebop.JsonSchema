namespace Bebop.JsonSchema.Assertions.Array;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class ContainsAssertion(JsonSchema schema, int? minContains, int? maxContains) : Assertion
{
    public override string[] AssociatedKeyword => ["contains", "minContains", "maxContains"];

    public override bool Assert(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind != JsonValueKind.Array)
            return true;

        if (minContains.HasValue && maxContains.HasValue)
            return _AssertWithMinMax(element, evaluationState, errorCollection, minContains.Value, maxContains.Value);

        if (minContains.HasValue && !maxContains.HasValue)
            return _AssertWithMin(element, evaluationState, errorCollection, minContains.Value);

        if (!minContains.HasValue && maxContains.HasValue)
            return _AssertWithMax(element, evaluationState, errorCollection, maxContains.Value);

        return _AssertWithoutMinMax(element, evaluationState, errorCollection);
    }

    private bool _AssertWithMinMax(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection, int min, int max)
    {
        int c = 0;
        int i = 0;
        var isValid = true;
        using var absorptionList = Pool.RentArray<EvaluationState>(element.Element.GetArrayLength());

        foreach (var e in element.Element.EnumerateArray())
        {
            var es = evaluationState.New();
            var isContainsValid = false;
            if (schema.Validate(element.Subitem(in e, i), es, errorCollection))
            {
                c++;
                if (c > max)
                {
                    errorCollection.AddError($"Array contains more than {max} valid items", element);
                    isValid = false;
                }

                isContainsValid = true;
                absorptionList.Add(es);
            }

            evaluationState.AddProperty(element.ElementPath.AppendIndex(i), isContainsValid);
            i++;
        }

        if (c < min)
        {
            errorCollection.AddError($"Array contains less than {min} valid items", element);
            return false;
        }

        foreach (var e in absorptionList.AsSpan())
        {
            evaluationState.Absorb(e);
        }

        return isValid;
    }

    private bool _AssertWithMax(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection, int max)
    {
        int c = 0;
        int i = 0;
        var isValid = true;
        using var absorptionList = Pool.RentArray<EvaluationState>(element.Element.GetArrayLength());

        foreach (var e in element.Element.EnumerateArray())
        {
            var es = evaluationState.New();
            var isContainsValid = false;
            if (schema.Validate(element.Subitem(in e, i), es, errorCollection))
            {
                c++;
                isContainsValid = true;
                if (c <= max) 
                {
                    i++;
                }
                else
                {
                    errorCollection.AddError($"Array contains more than {max} valid items", element);
                    isValid = false;
                }

                absorptionList.Add(es);
            }

            evaluationState.AddProperty(element.ElementPath.AppendIndex(i), isContainsValid);
            i++;
        }

        foreach (var e in absorptionList.AsSpan())
        {
            evaluationState.Absorb(e);
        }

        return c > 0 && isValid; // contains should find the element at least once
    }

    private bool _AssertWithMin(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection, int min)
    {
        int c = 0;
        int i = 0;
        var isValid = min == 0;
        using var absorptionList = Pool.RentArray<EvaluationState>(element.Element.GetArrayLength());

        foreach (var e in element.Element.EnumerateArray())
        {
            var es = evaluationState.New();
            var isContainsValid = false;
            if (schema.Validate(element.Subitem(in e, i), es, errorCollection))
            {
                c++;
                if (c >= min) 
                    isValid = true;

                isContainsValid = true;
                absorptionList.Add(es);
            }

            evaluationState.AddProperty(element.ElementPath.AppendIndex(i), isContainsValid);
            i++;
        }

        if (!isValid)
            errorCollection.AddError($"Array contains less than {min} valid items", element);

        foreach (var e in absorptionList.AsSpan())
        {
            evaluationState.Absorb(e);
        }

        return isValid;
    }

    private bool _AssertWithoutMinMax(in Token element, in EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        int i = 0;
        var isValid = false;
        using var absorptionList = Pool.RentArray<EvaluationState>(element.Element.GetArrayLength());

        foreach (var e in element.Element.EnumerateArray())
        {
            var es = evaluationState.New();
            var isContainsValid = false;
            if (schema.Validate(element.Subitem(in e, i), es, errorCollection))
            {
                isValid = true;
                absorptionList.Add(es);
                isContainsValid = true;
            }

            evaluationState.AddProperty(element.ElementPath.AppendIndex(i), isContainsValid);
            i++;
        }

        foreach (var e in absorptionList.AsSpan())
        {
            evaluationState.Absorb(e);
        }

        return isValid;
    }
}