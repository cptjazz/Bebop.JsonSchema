using Tests.Bebop.JsonSchema.Infrastructure;

namespace Tests.MyJsonSchema;

public class Test_Draft202012
{
    private sealed class SchemaResolver : ISchemaResolver
    {
        public JsonElement? Resolve(Uri id)
        {
            if (id.Host.EndsWith("schemas.json"))
                return null;

            if (id.Host == "localhost")
            {
                var file = id.PathAndQuery;
                var path = Path.Join(AppContext.BaseDirectory, "TestData/remotes", file);

                if (!File.Exists(path))
                    return null;

                using var stream = File.OpenRead(path);
                using var doc = JsonDocument.Parse(stream);
                return doc.RootElement.Clone();
            }

            var httpClient = new HttpClient();
            var response = httpClient.GetAsync(id).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                using var doc = JsonDocument.Parse(content);
                return doc.RootElement.Clone();
            }

            return null;
        }
    }

    [Theory]
    [ClassData(typeof(Draft202012TestData))]
    public void RequiredTestCases(TestData data)
    {
        var repo = SchemaRegistry.Custom(new SchemaResolver());
        var jsonSchema = JsonSchema.Create(data.Schema, repo);
        var errorCollection = new ErrorCollection();
        var result = jsonSchema.Validate(data.Data, errorCollection);
        
        Assert.Equal(data.ExpectedValid, result);
    }

    [Theory]
    [ClassData(typeof(Draft202012OptionalTestData))]
    public void OptionalTestCases(TestData data)
    {
        var repo = SchemaRegistry.Resolving();
        var jsonSchema = JsonSchema.Create(data.Schema, repo);
        var errorCollection = new ErrorCollection();
        var result = jsonSchema.Validate(data.Data, errorCollection);
        
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
                         "$schema": "http://localhost:1234/draft2020-12/metaschema-no-validation.json",
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

        var repo = SchemaRegistry.Custom(new SchemaResolver());
        var root = doc.RootElement;

        var schemaJson = root.GetProperty("schema");
        var schema = JsonSchema.Create(schemaJson, repo);
        await schema.Prepare();

        foreach (var test in root.GetProperty("tests").EnumerateArray())
        {
            var data = test.GetProperty("data");
            var expected = test.GetProperty("valid").GetBoolean();

            var errorCollection = new ErrorCollection();
            var result = schema.Validate(data, errorCollection);
            Assert.Equal(expected, result);
        }
    }
}