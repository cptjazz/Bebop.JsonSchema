using System.Collections;
using Tests.Bebop.JsonSchema.Infrastructure;

namespace Tests.MyJsonSchema;

public sealed class Draft202012OptionalTestData : TestDataBase, IEnumerable<object[]>
{
    private const string TestDataPath = "TestSuite/tests/draft2020-12/optional";

    public IEnumerator<object[]> GetEnumerator()
    {
        var di = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, TestDataPath));
        var files = di.GetFiles("*.json");

        files = files
            // Exclude because .NET's regex engine does not properly support higher planes
            .Where(f => !f.Name.Equals("non-bmp-regex.json", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return GetTests(files);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
