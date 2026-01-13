namespace Tests.MyJsonSchema;

public class Test_StringProperties
{
    [Fact]
    public void Test_MinLength()
    {
        var schema = JsonSchema.Create("""
                                       {
                                           "type": "string",
                                           "minLength": 2
                                       }
                                       """);

        var result = schema.Validate(JsonDocument.Parse("\"a\""), new ErrorCollection());
        Assert.False(result);
    }

    [Fact]
    public void Test_MaxLength()
    {
        var schema = JsonSchema.Create("""
                                       {
                                           "type": "string",
                                           "maxLength": 2
                                       }
                                       """);

        var result = schema.Validate(JsonDocument.Parse("\"abc\""), new ErrorCollection());
        Assert.False(result);
    }

    [Fact]
    public void Test_Pattern()
    {
        var schema = JsonSchema.Create("""
                                       {
                                           "type": "string",
                                           "pattern": "^[a-z]+$"
                                       }
                                       """);

        var result = schema.Validate(JsonDocument.Parse("\"ABC\""), new ErrorCollection());
        Assert.False(result);
    }
}