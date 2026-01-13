namespace Bebop.JsonSchema.Schemas;

internal static class DefaultSchemas
{
    public static IEnumerable<JsonSchema> Get()
    {
        yield return _Get("Draft202012.core.json");
        yield return _Get("Draft202012.applicator.json");
        yield return _Get("Draft202012.validation.json");
        yield return _Get("Draft202012.unevaluated.json");
        yield return _Get("Draft202012.content.json");
        yield return _Get("Draft202012.format-annotation.json");
        yield return _Get("Draft202012.meta-data.json");
        yield return _Get("Draft202012.schema.json");

    }

    private static JsonSchema _Get(string file)
    {
        using var stream = typeof(DefaultSchemas)
            .Assembly
            .GetManifestResourceStream($"MyJsonSchema.Schemas.{file}");

        using var doc = JsonDocument.Parse(stream!);

        return JsonSchema.Create(doc);
    }
}