namespace Bebop.JsonSchema;

public class ErrorCollection
{
    private readonly List<(string Error, JsonElement Element, JsonPointer Path)> _errors = new(13);

    internal virtual void AddError(string error, in Token element)
    {
        _errors.Add((error, element.Element, element.ElementPath));
    }
}

internal sealed class NopErrorCollection : ErrorCollection
{
    public static readonly NopErrorCollection Instance = new();

    internal override void AddError(string error, in Token element)
    {
    }
}
