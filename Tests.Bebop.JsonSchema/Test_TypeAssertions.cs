namespace Tests.MyJsonSchema;

public class Test_TypeAssertions
{
    [Fact]
    public void String()
    {
        var data = """
                   "a string"
                   """;

        var schema = """
                     {
                         "type": "string"
                     }
                     """;

        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);

        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());

        Assert.True(result);
    }

    [Fact]
    public void Boolean()
    {
        var data = """
                   true
                   """;
        var schema = """
                     {
                         "type": "boolean"
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());

        Assert.True(result);
    }

    [Fact]
    public void Number()
    {
        var data = """
                   42
                   """;
        var schema = """
                     {
                         "type": "number"
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());

        Assert.True(result);
    }

    [Fact]
    public void Object()
    {
        var data = """
                   {
                       "key": "value"
                   }
                   """;
        var schema = """
                     {
                         "type": "object"
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());

        Assert.True(result);
    }

    [Fact]
    public void Array()
    {
        var data = """
                   [1, 2, 3]
                   """;
        var schema = """
                     {
                         "type": "array"
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());

        Assert.True(result);
    }

    [Fact]
    public void Null()
    {
        var data = """
                   null
                   """;
        var schema = """
                     {
                         "type": "null"
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());

        Assert.True(result);
    }

    [Fact]
    public void Any()
    {
        var data = """
                   12345
                   """;
        var schema = """
                     true
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());
        Assert.True(result);
    }

    [Fact]
    public void None()
    {
        var data = """
                   12345
                   """;
        var schema = """
                     false
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());
        Assert.False(result);
    }
}