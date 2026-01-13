namespace Tests.MyJsonSchema;

public record TestData(
    string FileName,
    string SchemaDescription,
    string TestDescription,
    JsonElement Schema,
    JsonElement Data,
    bool ExpectedValid
)
{
    public override string ToString()
    {
        return $"({FileName}) : {SchemaDescription} - {TestDescription}";
    }
}