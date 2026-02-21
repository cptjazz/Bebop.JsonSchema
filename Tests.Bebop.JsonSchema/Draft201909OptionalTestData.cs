using System.Collections;
using Tests.Bebop.JsonSchema.Infrastructure;

namespace Tests.MyJsonSchema;

public sealed class Draft201909OptionalTestData : TestDataBase, IEnumerable<object[]>
{
    private const string TestDataPath = "TestSuite/tests/draft2019-09/optional";

    public IEnumerator<object[]> GetEnumerator()
    {
        var di = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, TestDataPath));
        var files = di.GetFiles("*.json");

        files = files
            // Exclude because .NET's regex engine does not properly support higher planes
            .Where(f => !f.Name.Equals("non-bmp-regex.json", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Name.Equals("cross-draft.json", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return GetTests(files);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
