namespace Bebop.JsonSchema.Assertions;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Validation)]
internal sealed class ConstAssertion(JsonElement constant) : PreparedAssertion
{
    private readonly IJsonValueComparer _comparer = _GetComparer(constant);

    public override string[] AssociatedKeyword => ["const"];

    private static IJsonValueComparer _GetComparer(in JsonElement c)
    {
        return c.ValueKind switch
        {
            JsonValueKind.String => new JsonStringComparer(c.GetString()),
            JsonValueKind.Number => new JsonDoubleComparer(c.GetDouble()),
            JsonValueKind.True or JsonValueKind.False => new JsonBooleanComparer(c.GetBoolean()),
            JsonValueKind.Null => JsonNullComparer.Instance,
            JsonValueKind.Object => new JsonObjectComparer(c),
            JsonValueKind.Array => new JsonArrayComparer(c),
            _ => throw new InvalidOperationException("Unsupported constant type.")
        };
    }

    public override ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        if (element.Element.ValueKind == constant.ValueKind)
        {
            if (_comparer.AreEqual(element.Element))
                return ValueTask.FromResult(true);
        }

        return ValueTask.FromResult(_AddError(element, errorCollection));

        static bool _AddError(in Token e, ErrorCollection ec)
        {
            ec.AddError("Element does not match the constant value.", e);
            return false;
        }
    }
}