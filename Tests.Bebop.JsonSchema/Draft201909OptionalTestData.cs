using System.Collections;
using Tests.Bebop.JsonSchema.Infrastructure;

namespace Tests.MyJsonSchema;

public sealed class Draft201909OptionalTestData : TestDataBase, IEnumerable<object[]>
{
    private const string TestDataPath = @"TestData\draft2019-09\optional";

    public IEnumerator<object[]> GetEnumerator()
    {
        var di = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, TestDataPath));
        var files = di.GetFiles("*.json");

        return GetTests(files);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
