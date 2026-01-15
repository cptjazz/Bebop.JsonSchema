namespace Bebop.JsonSchema;

internal abstract class Dialect
{
    internal static readonly Uri DefaultDialectUri = new Uri("https://json-schema.org/draft/2020-12/schema", UriKind.Absolute);

    internal static Dialect? Get(Uri schemaUri)
    {
        if (schemaUri == DefaultDialectUri)
        {
            return Dialect_Draft202012.Instance;
        }

        return null;
    }

    internal static Dialect FromSchema(JsonSchema ds)
    {
        return new CustomDialect(ds.Dialect, ds);
    }

    public abstract IReadOnlySet<string> SupportedKeywords { get; } 

    public abstract IReadOnlySet<string> GetKeywordSet(Uri vocabularyUri);
}