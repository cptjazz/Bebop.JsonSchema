namespace Tests.Bebop.JsonSchema.Infrastructure;

public class TestDataBase
{
    protected static IEnumerator<object[]> GetTests(FileInfo[] files)
    {
        foreach (var file in files)
        {
            using var stream = file.OpenRead();
            using var doc = JsonDocument.Parse(stream);

            foreach (var testGroup in doc.RootElement.EnumerateArray())
            {
                var schemaDescription = testGroup.GetProperty("description").GetString() ?? "Unknown schema";
                var schemaJson = testGroup.GetProperty("schema");

                foreach (var test in testGroup.GetProperty("tests").EnumerateArray())
                {
                    var testDescription = test.GetProperty("description").GetString() ?? "Unknown test";
                    var data = test.GetProperty("data");
                    var expectedValid = test.GetProperty("valid").GetBoolean();

                    // Clone the JsonElements so they persist beyond the using block
                    var schemaClone = JsonSerializer.Deserialize<JsonElement>(schemaJson.GetRawText());
                    var dataClone = JsonSerializer.Deserialize<JsonElement>(data.GetRawText());

                    yield return
                    [ new TestData(
                        file.Name,
                        schemaDescription,
                        testDescription,
                        schemaClone,
                        dataClone,
                        expectedValid)];
                }
            }
        }
    }
}