using System.Text.Json;
using Bebop.JsonSchema;

namespace Tests.MyJsonSchema;

public class Test_JsonExtensions
{
    // ── Helpers ──────────────────────────────────────────────────────

    private static JsonElement Element(string json)
    {
        return JsonDocument.Parse(json).RootElement;
    }

    private static JsonProperty Property(string json)
    {
        var doc = JsonDocument.Parse(json);
        foreach (var prop in doc.RootElement.EnumerateObject())
            return prop;

        throw new InvalidOperationException("No property found.");
    }

    // ── ExpectObject(JsonElement) ───────────────────────────────────

    [Fact]
    public void ExpectObject_Element_ReturnsElement_WhenObject()
    {
        var el = Element("""{"a":1}""");
        var result = el.ExpectObject();
        Assert.Equal(JsonValueKind.Object, result.ValueKind);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("\"hello\"")]
    [InlineData("[1,2]")]
    [InlineData("true")]
    [InlineData("null")]
    public void ExpectObject_Element_Throws_WhenNotObject(string json)
    {
        var el = Element(json);
        Assert.Throws<InvalidSchemaException>(() => el.ExpectObject());
    }

    // ── ExpectObject(JsonProperty) ──────────────────────────────────

    [Fact]
    public void ExpectObject_Property_ReturnsElement_WhenObject()
    {
        var prop = Property("""{"p":{"a":1}}""");
        var result = prop.ExpectObject();
        Assert.Equal(JsonValueKind.Object, result.ValueKind);
    }

    [Fact]
    public void ExpectObject_Property_Throws_WhenNotObject()
    {
        var prop = Property("""{"p":123}""");
        Assert.Throws<InvalidSchemaException>(() => prop.ExpectObject());
    }

    // ── ExpectUri ───────────────────────────────────────────────────

    [Fact]
    public void ExpectUri_ReturnsUri_WhenString()
    {
        var el = Element("""  "https://example.com" """);
        var uri = el.ExpectUri();
        Assert.Equal("https://example.com", uri.OriginalString);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("true")]
    [InlineData("null")]
    public void ExpectUri_Throws_WhenNotString(string json)
    {
        var el = Element(json);
        Assert.Throws<InvalidSchemaException>(() => el.ExpectUri());
    }

    // ── ExpectNumber(JsonProperty) ──────────────────────────────────

    [Fact]
    public void ExpectNumber_ReturnsDouble_WhenNumber()
    {
        var prop = Property("""{"p":3.14}""");
        Assert.Equal(3.14, prop.ExpectNumber());
    }

    [Fact]
    public void ExpectNumber_Throws_WhenNotNumber()
    {
        var prop = Property("""{"p":"hello"}""");
        Assert.Throws<InvalidSchemaException>(() => prop.ExpectNumber());
    }

    // ── ExpectNonNegativeCount(JsonProperty) ────────────────────────

    [Fact]
    public void ExpectNonNegativeCount_ReturnsInt_WhenNonNegativeInt()
    {
        var prop = Property("""{"p":5}""");
        Assert.Equal(5, prop.ExpectNonNegativeCount());
    }

    [Fact]
    public void ExpectNonNegativeCount_ReturnsInt_WhenWholeDouble()
    {
        // 1e2 = 100.0 — a whole number represented as a double that doesn't parse as int32
        var prop = Property("""{"p":1e2}""");
        Assert.Equal(100, prop.ExpectNonNegativeCount());
    }

    [Fact]
    public void ExpectNonNegativeCount_Throws_WhenNotNumber()
    {
        var prop = Property("""{"p":"abc"}""");
        Assert.Throws<InvalidSchemaException>(() => prop.ExpectNonNegativeCount());
    }

    [Fact]
    public void ExpectNonNegativeCount_Throws_WhenNegativeInt()
    {
        var prop = Property("""{"p":-3}""");
        Assert.Throws<InvalidSchemaException>(() => prop.ExpectNonNegativeCount());
    }

    [Fact]
    public void ExpectNonNegativeCount_Throws_WhenFractionalDouble()
    {
        var prop = Property("""{"p":2.5}""");
        Assert.Throws<InvalidSchemaException>(() => prop.ExpectNonNegativeCount());
    }

    [Fact]
    public void ExpectNonNegativeCount_Throws_WhenNegativeDouble()
    {
        var prop = Property("""{"p":-1.0e1}""");
        Assert.Throws<InvalidSchemaException>(() => prop.ExpectNonNegativeCount());
    }

    // ── ExpectBoolean(JsonProperty) ─────────────────────────────────

    [Theory]
    [InlineData("""{"p":true}""", true)]
    [InlineData("""{"p":false}""", false)]
    public void ExpectBoolean_Property_ReturnsValue_WhenBoolean(string json, bool expected)
    {
        var prop = Property(json);
        Assert.Equal(expected, prop.ExpectBoolean());
    }

    [Fact]
    public void ExpectBoolean_Property_Throws_WhenNotBoolean()
    {
        var prop = Property("""{"p":42}""");
        Assert.Throws<InvalidSchemaException>(() => prop.ExpectBoolean());
    }

    // ── ExpectBoolean(JsonElement) ──────────────────────────────────

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void ExpectBoolean_Element_ReturnsValue_WhenBoolean(string json, bool expected)
    {
        var el = Element(json);
        Assert.Equal(expected, el.ExpectBoolean());
    }

    [Theory]
    [InlineData("123")]
    [InlineData("\"text\"")]
    [InlineData("null")]
    public void ExpectBoolean_Element_Throws_WhenNotBoolean(string json)
    {
        var el = Element(json);
        Assert.Throws<InvalidSchemaException>(() => el.ExpectBoolean());
    }

    // ── ExpectArray(JsonElement) ────────────────────────────────────

    [Fact]
    public void ExpectArray_Element_ReturnsEnumerator_WhenArray()
    {
        var el = Element("[1,2,3]");
        var enumerator = el.ExpectArray();
        var count = 0;
        foreach (var _ in enumerator) count++;
        Assert.Equal(3, count);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("{}")]
    [InlineData("\"text\"")]
    public void ExpectArray_Element_Throws_WhenNotArray(string json)
    {
        var el = Element(json);
        Assert.Throws<InvalidSchemaException>(() => el.ExpectArray());
    }

    // ── ExpectArray(JsonProperty) ───────────────────────────────────

    [Fact]
    public void ExpectArray_Property_ReturnsEnumerator_WhenArray()
    {
        var prop = Property("""{"p":[1,2]}""");
        var enumerator = prop.ExpectArray();
        var count = 0;
        foreach (var _ in enumerator) count++;
        Assert.Equal(2, count);
    }

    [Fact]
    public void ExpectArray_Property_Throws_WhenNotArray()
    {
        var prop = Property("""{"p":"hello"}""");
        Assert.Throws<InvalidSchemaException>(() => prop.ExpectArray());
    }

    // ── ExpectNonNegativeNumber(JsonElement) ────────────────────────

    [Fact]
    public void ExpectNonNegativeNumber_ReturnsInt_WhenNonNegativeInt()
    {
        var el = Element("7");
        Assert.Equal(7, el.ExpectNonNegativeNumber());
    }

    [Fact]
    public void ExpectNonNegativeNumber_ReturnsInt_WhenWholeDouble()
    {
        var el = Element("3.0");
        Assert.Equal(3, el.ExpectNonNegativeNumber());
    }

    [Fact]
    public void ExpectNonNegativeNumber_Throws_WhenNotNumber()
    {
        var el = Element("\"abc\"");
        Assert.Throws<InvalidSchemaException>(() => el.ExpectNonNegativeNumber());
    }

    [Fact]
    public void ExpectNonNegativeNumber_Throws_WhenNegativeInt()
    {
        var el = Element("-5");
        Assert.Throws<InvalidSchemaException>(() => el.ExpectNonNegativeNumber());
    }

    [Fact]
    public void ExpectNonNegativeNumber_Throws_WhenFractionalDouble()
    {
        var el = Element("2.5");
        Assert.Throws<InvalidSchemaException>(() => el.ExpectNonNegativeNumber());
    }

    [Fact]
    public void ExpectNonNegativeNumber_Throws_WhenNegativeDouble()
    {
        var el = Element("-1.5");
        Assert.Throws<InvalidSchemaException>(() => el.ExpectNonNegativeNumber());
    }

    // ── ExpectString(JsonProperty) ──────────────────────────────────

    [Fact]
    public void ExpectString_Property_ReturnsString_WhenString()
    {
        var prop = Property("""{"p":"hello"}""");
        Assert.Equal("hello", prop.ExpectString());
    }

    [Fact]
    public void ExpectString_Property_Throws_WhenNotString()
    {
        var prop = Property("""{"p":42}""");
        Assert.Throws<InvalidSchemaException>(() => prop.ExpectString());
    }

    // ── ExpectString(JsonElement) ───────────────────────────────────

    [Fact]
    public void ExpectString_Element_ReturnsString_WhenString()
    {
        var el = Element(""" "world" """);
        Assert.Equal("world", el.ExpectString());
    }

    [Theory]
    [InlineData("123")]
    [InlineData("true")]
    [InlineData("null")]
    [InlineData("[1]")]
    public void ExpectString_Element_Throws_WhenNotString(string json)
    {
        var el = Element(json);
        Assert.Throws<InvalidSchemaException>(() => el.ExpectString());
    }
}
