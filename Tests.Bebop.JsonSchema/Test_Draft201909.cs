using Tests.Bebop.JsonSchema.Infrastructure;

namespace Tests.MyJsonSchema;

public class Test_Draft201909
{
    private static readonly CachingSchemaResolver SchemaResolver = new();

    [Theory]
    [ClassData(typeof(Draft201909TestData))]
    public async Task RequiredTestCases(TestData data)
    {
        var repo = SchemaRegistry.Custom(SchemaResolver);
        var jsonSchema = await JsonSchema.Create(data.Schema, repo);
        var errorCollection = new ErrorCollection();

        await jsonSchema.Prepare();
        var result = await jsonSchema.Validate(data.Data, errorCollection);
        
        Assert.Equal(data.ExpectedValid, result);
    }

    [Theory]
    [ClassData(typeof(Draft201909OptionalTestData))]
    public async Task OptionalTestCases(TestData data)
    {
        var repo = SchemaRegistry.Custom(SchemaResolver);
        var jsonSchema = await JsonSchema.Create(data.Schema, repo);
        var errorCollection = new ErrorCollection();
        
        await jsonSchema.Prepare();
        var result = await jsonSchema.Validate(data.Data, errorCollection);
        
        Assert.Equal(data.ExpectedValid, result);
    }

    [Fact(Skip = "For debug only")]
    public async Task DebuggableTest()
    {
        var json="""
                 {
                     "description": "schema that uses custom metaschema with with no validation vocabulary",
                     "schema": {
                         "$id": "https://schema/using/no/validation",
                         "$schema": "http://localhost:1234/draft2019-09/metaschema-no-validation.json",
                         "properties": {
                             "badProperty": false,
                             "numberProperty": {
                                 "minimum": 10
                             }
                         }
                     },
                     "tests": [
                         {
                             "description": "no validation: invalid number, but it still validates",
                             "data": {
                                 "numberProperty": 1
                             },
                             "valid": true
                         }
                     ]
                 }
                 """;
        
        using var doc = JsonDocument.Parse(json);

        var repo = SchemaRegistry.Custom(SchemaResolver);
        var root = doc.RootElement;

        var schemaJson = root.GetProperty("schema");
        var schema = await JsonSchema.Create(schemaJson, repo);
        await schema.Prepare();

        var tests = root.GetProperty("tests");
        foreach (var test in tests.EnumerateArray())
        {
            var data = test.GetProperty("data");
            var expected = test.GetProperty("valid").GetBoolean();

            var errorCollection = new ErrorCollection();
            var result = await schema.Validate(data, errorCollection);

            Assert.Equal(expected, result);
        }
    }
}
