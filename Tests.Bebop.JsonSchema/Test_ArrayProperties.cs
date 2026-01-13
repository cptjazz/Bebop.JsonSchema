namespace Tests.MyJsonSchema;

public class Test_ArrayProperties
{
    [Fact]
    public void MinItems()
    {
        var data = """
                   [1, 2]
                   """;
        var schema = """
                     {
                         "type": "array",
                         "minItems": 3
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());
        Assert.False(result);
    }

    [Fact]
    public void MaxItems()
    {
        var data = """
                   [1, 2, 3, 4]
                   """;
        var schema = """
                     {
                         "type": "array",
                         "maxItems": 3
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());
        Assert.False(result);
    }

    [Fact]
    public void BothMinAndMaxItems()
    {
        var data = """
                   [1, 2, 3]
                   """;
        var schema = """
                     {
                         "type": "array",
                         "minItems": 2,
                         "maxItems": 4
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());
        Assert.True(result);
    }

    [Fact]
    public void UniqueItems_Pass()
    {
        var data = """
                   [1, 2, 3, 4]
                   """;
        var schema = """
                     {
                         "type": "array",
                         "uniqueItems": true
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());
        Assert.True(result);
    }

    [Fact]
    public void UniqueItems_Fail()
    {
        var data = """
                   [1, 2, 2, 3]
                   """;
        var schema = """
                     {
                         "type": "array",
                         "uniqueItems": true
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());
        Assert.False(result);
    }
}