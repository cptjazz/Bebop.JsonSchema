namespace Bebop.JsonSchema;

/// <summary>
/// Represents a validation error with its message, the JSON element that failed validation, and the path to that element.
/// </summary>
/// <param name="Message">The error message describing what validation failed.</param>
/// <param name="Element">The JSON element that failed validation.</param>
/// <param name="Path">The JSON Pointer path to the element that failed validation.</param>
public sealed record ValidationError(string Message, JsonElement Element, string Path);

/// <summary>
/// A collection of validation errors produced during JSON Schema validation.
/// </summary>
public class ErrorCollection : IReadOnlyList<ValidationError>
{
    private readonly List<(string Error, JsonElement Element, JsonPointer Path)> _errors = new(13);
    private List<ValidationError>? _cachedErrors;

    /// <summary>
    /// Gets the number of validation errors in the collection.
    /// </summary>
    public int Count => _errors.Count;

    /// <summary>
    /// Gets the validation error at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the error to get.</param>
    /// <returns>The validation error at the specified index.</returns>
    public ValidationError this[int index]
    {
        get
        {
            EnsureCachedErrors();
            return _cachedErrors![index];
        }
    }

    internal virtual void AddError(string error, in Token element)
    {
        _errors.Add((error, element.Element, element.ElementPath));
        _cachedErrors = null; // Invalidate cache when errors are added
    }

    /// <summary>
    /// Returns an enumerator that iterates through the validation errors.
    /// </summary>
    /// <returns>An enumerator for the validation errors.</returns>
    public IEnumerator<ValidationError> GetEnumerator()
    {
        EnsureCachedErrors();
        return _cachedErrors!.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void EnsureCachedErrors()
    {
        if (_cachedErrors == null)
        {
            _cachedErrors = new List<ValidationError>(_errors.Count);
            foreach (var error in _errors)
            {
                _cachedErrors.Add(new ValidationError(error.Error, error.Element, error.Path.ToString()));
            }
        }
    }
}

internal sealed class NopErrorCollection : ErrorCollection
{
    public static readonly NopErrorCollection Instance = new();

    internal override void AddError(string error, in Token element)
    {
    }
}
