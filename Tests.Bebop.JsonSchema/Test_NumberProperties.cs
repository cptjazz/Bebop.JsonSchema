namespace Tests.MyJsonSchema;

public class Test_NumberProperties
{
    [Fact]
    public void Test_ExclusiveMinimum()
    {
        var schema = JsonSchema.Create("""
                                       { 
                                           "type": "number", 
                                           "exclusiveMinimum": 0 
                                       }
                                       """);

        var result = schema.Validate(JsonDocument.Parse("-1"), new ErrorCollection());
        Assert.False(result);
    }

    [Fact]
    public void Test_ExclusiveMaximum()
    {
        var schema = JsonSchema.Create("""
                                       { 
                                            "type": "number", 
                                            "exclusiveMaximum": 0 
                                       }
                                       """);

        var result = schema.Validate(JsonDocument.Parse("1"), new ErrorCollection());
        Assert.False(result);
    }

    [Fact]
    public void Test_MultipleOf()
    {
        var schema = JsonSchema.Create("""
                                       {
                                           "type": "number",
                                           "multipleOf": 2
                                       }
                                       """);

        var result = schema.Validate(JsonDocument.Parse("3"), new ErrorCollection());
        Assert.False(result);
    }
}