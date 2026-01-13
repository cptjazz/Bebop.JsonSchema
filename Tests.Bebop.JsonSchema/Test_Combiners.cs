namespace Tests.MyJsonSchema;

public class Test_Combiners
{
    [Fact]
    public void Test_AnyOf_Success1()
    {
        var data = """
                   "ok"
                   """;

        var schema = """
                     {
                       "anyOf": [
                         { "type": "string", "maxLength": 5 },
                         { "type": "number", "minimum": 0 }
                       ]
                     }
                     """;

        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var errorCollection = new ErrorCollection();
        var result = jsonSchema.Validate(dataDocument, errorCollection);
        Assert.True(result);
    }

    [Fact]
    public void Test_AnyOf_Success2()
    {
        var data = """
                   42
                   """;

        var schema = """
                     {
                       "anyOf": [
                         { "type": "string", "maxLength": 5 },
                         { "type": "number", "minimum": 0 }
                       ]
                     }
                     """;

        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var errorCollection = new ErrorCollection();
        var result = jsonSchema.Validate(dataDocument, errorCollection);
        Assert.True(result);
    }

    [Fact]
    public void Test_AnyOf_Failure1()
    {
        var data = """
                   "too long"
                   """;

        var schema = """
                     {
                       "anyOf": [
                         { "type": "string", "maxLength": 5 },
                         { "type": "number", "minimum": 0 }
                       ]
                     }
                     """;

        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var errorCollection = new ErrorCollection();
        var result = jsonSchema.Validate(dataDocument, errorCollection);
        Assert.False(result);
    }

    [Fact]
    public void Test_AnyOf_Failure2()
    {
        var data = """
                   -42
                   """;

        var schema = """
                     {
                       "anyOf": [
                         { "type": "string", "maxLength": 5 },
                         { "type": "number", "minimum": 0 }
                       ]
                     }
                     """;

        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var errorCollection = new ErrorCollection();
        var result = jsonSchema.Validate(dataDocument, errorCollection);
        Assert.False(result);
    }

    [Fact]
    public void Test_OneOf_Success1()
    {
        var data = """
                   10
                   """;

        var schema = """
                     {
                       "oneOf": [
                         { "type": "number", "multipleOf": 5 },
                         { "type": "number", "multipleOf": 3 }
                       ]
                     }
                     """;

        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var errorCollection = new ErrorCollection();
        var result = jsonSchema.Validate(dataDocument, errorCollection);
        Assert.True(result);
    }

    [Fact]
    public void Test_OneOf_Success2()
    {
        var data = """
                   9
                   """;

        var schema = """
                     {
                       "oneOf": [
                         { "type": "number", "multipleOf": 5 },
                         { "type": "number", "multipleOf": 3 }
                       ]
                     }
                     """;

        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var errorCollection = new ErrorCollection();
        var result = jsonSchema.Validate(dataDocument, errorCollection);
        Assert.True(result);
    }

    [Fact]
    public void Test_OneOf_Failure1()
    {
        var data = """
                   2
                   """;

        var schema = """
                     {
                       "oneOf": [
                         { "type": "number", "multipleOf": 5 },
                         { "type": "number", "multipleOf": 3 }
                       ]
                     }
                     """;

        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var errorCollection = new ErrorCollection();
        var result = jsonSchema.Validate(dataDocument, errorCollection);
        Assert.False(result);
    }

    [Fact]
    public void Test_OneOf_Failure2()
    {
        var data = """
                   15
                   """;

        var schema = """
                     {
                       "oneOf": [
                         { "type": "number", "multipleOf": 5 },
                         { "type": "number", "multipleOf": 3 }
                       ]
                     }
                     """;

        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var errorCollection = new ErrorCollection();
        var result = jsonSchema.Validate(dataDocument, errorCollection);
        Assert.False(result);
    }

    [Fact]
    public void Test_AllOf_Success()
    {
        var data = """
                   "ok"
                   """;

        var schema = """
                     {
                       "allOf": [
                         { "type": "string" },
                         { "maxLength": 5 }
                       ]
                     }
                     """;

        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var errorCollection = new ErrorCollection();
        var result = jsonSchema.Validate(dataDocument, errorCollection);
        Assert.True(result);
    }

    [Fact]
    public void Test_AllOf_Failure()
    {
        var data = """
                   "too long"
                   """;

        var schema = """
                     {
                       "allOf": [
                         { "type": "string" },
                         { "maxLength": 5 }
                       ]
                     }
                     """;

        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var errorCollection = new ErrorCollection();
        var result = jsonSchema.Validate(dataDocument, errorCollection);
        Assert.True(result);
    }
}