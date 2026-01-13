using System.Collections;

namespace Tests.MyJsonSchema;

public sealed class Draft202012TestData : TestDataBase, IEnumerable<object[]>
{
    private const string TestDataPath = @"TestData\draft2020-12";

    public IEnumerator<object[]> GetEnumerator()
    {
        var di = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, TestDataPath));
        var files = di.GetFiles("*.json");

        return GetTests(files);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}