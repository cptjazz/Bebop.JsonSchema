namespace Tests.MyJsonSchema;

public class Test_ObjectProperties
{
    [Fact]
    public void Test_RequiredPropertiesPropertyAssertion()
    {
        var data = """
                     {
                          "name": "John",
                          "age": 30
                     }
                   """;
        var schema = """
                     {
                          "type": "object",
                          "properties": {
                              "name": { "type": "string" },
                              "age": { "type": "integer", "minimum": 0 }
                          },
                          "required": ["name", "age"]
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());
        Assert.True(result);
    }

    [Fact]
    public void Test_OptionalPropertiesPropertyAssertion()
    {
        var data = """
                     {
                          "name": "John"
                     }
                   """;
        var schema = """
                     {
                          "type": "object",
                          "properties": {
                              "name": { "type": "string" },
                              "age": { "type": "integer", "minimum": 0 }
                          },
                          "required": ["name"]
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());
        Assert.True(result);
    }

    [Fact]
    public void Test_OptionalPropertiesPropertyAssertion2()
    {
        var data = """
                     {
                          "name": "John",
                          "age": -5
                     }
                   """;
        var schema = """
                     {
                          "type": "object",
                          "properties": {
                              "name": { "type": "string" },
                              "age": { "type": "integer", "minimum": 0 }
                          },
                          "required": ["name"]
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
    public void Test_OptionalPropertiesPropertyAssertion3()
    {
        var data = """
                     {
                          "name": "John",
                          "age": 25
                     }
                   """;
        var schema = """
                     {
                          "type": "object",
                          "properties": {
                              "name": { "type": "string" },
                              "age": { "type": "integer", "minimum": 0 }
                          },
                          "required": ["name"]
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());
        Assert.True(result);
    }

    [Fact]
    public void ComplexSchema()
    {
                var data = """
                     {
                          "name": "John",
                          "age": 30,
                          "address": {
                              "street": "123 Main St",
                              "city": "Anytown"
                          }
                     }
                   """;
        var schema = """
                     {
                          "type": "object",
                          "properties": {
                              "name": { "type": "string" },
                              "age": { "type": "integer", "minimum": 0 },
                              "address": {
                                  "type": "object",
                                  "properties": {
                                      "street": { "type": "string" },
                                      "city": { "type": "string" }
                                  },
                                  "required": ["street", "city"]
                              }
                          },
                          "required": ["name", "age", "address"]
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());
        Assert.True(result);
    }

    [Fact]
    public void VeryComplexSchema_Success()
    {
                var data = """
                     {
                          "name": "John",
                          "age": 30,
                          "address": {
                              "street": "123 Main St",
                              "city": "Anytown"
                          }
                     }
                   """;
        var schema = """
                     {
                          "type": "object",
                          "properties": {
                              "name": { "type": "string" },
                              "age": { "type": "integer", "minimum": 0 },
                              "address": {
                                  "type": "object",
                                  "properties": {
                                      "street": { "type": "string" },
                                      "city": { "type": "string" }
                                  },
                                  "required": ["street", "city"]
                              }
                          },
                          "required": ["name", "age", "address"]
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var result = jsonSchema.Validate(dataDocument, new ErrorCollection());
        Assert.True(result);
    }

    [Fact]
    public void VeryComplexSchema_Failure()
    {
                var data = """
                     {
                          "name": "John",
                          "age": -5,
                          "address": {
                              "street": "123 Main St"
                          }
                     }
                   """;
        var schema = """
                     {
                          "type": "object",
                          "properties": {
                              "name": { "type": "string" },
                              "age": { "type": "integer", "minimum": 0 },
                              "address": {
                                  "type": "object",
                                  "properties": {
                                      "street": { "type": "string" },
                                      "city": { "type": "string" }
                                  },
                                  "required": ["street", "city"]
                              }
                          },
                          "required": ["name", "age", "address"]
                     }
                     """;
        var dataDocument = JsonDocument.Parse(data);
        var schemaDocument = JsonDocument.Parse(schema);
        var jsonSchema = JsonSchema.Create(schemaDocument);
        var errorCollection = new ErrorCollection();
        var result = jsonSchema.Validate(dataDocument, errorCollection);
        Assert.False(result);
    }
}