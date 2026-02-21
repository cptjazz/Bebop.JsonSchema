namespace Bebop.JsonSchema;

internal abstract class Dialect
{
    internal static readonly Uri Draft202012DialectUri = new Uri("https://json-schema.org/draft/2020-12/schema", UriKind.Absolute);
    internal static readonly Uri Draft201909DialectUri = new Uri("https://json-schema.org/draft/2019-09/schema", UriKind.Absolute);
    internal static readonly Uri Draft7DialectUri = new Uri("http://json-schema.org/draft-07/schema#", UriKind.Absolute);
    internal static readonly Uri Draft6DialectUri = new Uri("http://json-schema.org/draft-06/schema#", UriKind.Absolute);
    internal static readonly Uri Draft4DialectUri = new Uri("http://json-schema.org/draft-04/schema#", UriKind.Absolute);

    internal static readonly Uri DefaultDialectUri = Draft202012DialectUri;

    internal static Dialect? Get(Uri schemaUri)
    {
        return schemaUri switch
        {
            _ when schemaUri == DefaultDialectUri => Dialect_Draft202012.Instance,
            _ when schemaUri == Draft201909DialectUri => Dialect_Draft201909.Instance,
            _ when schemaUri == Draft7DialectUri => throw new NotSupportedException("Schema version Draft-07 is not supported."),
            _ when schemaUri == Draft6DialectUri => throw new NotSupportedException("Schema version Draft-06 is not supported."),
            _ when schemaUri == Draft4DialectUri => throw new NotSupportedException("Schema version Draft-04 is not supported."),
            _ => null
        };
    }

    internal static Dialect FromSchema(JsonSchema ds)
    {
        return new CustomDialect(ds.Dialect, ds);
    }

    public abstract IReadOnlySet<string> SupportedKeywords { get; } 

    public abstract IReadOnlySet<string> GetKeywordSet(Uri vocabularyUri);

    public abstract bool TryGetKeywordSet(Uri vocabularyUri, out IReadOnlySet<string> keywordSet);

    public virtual bool IsDraft202012 => false;
    public virtual bool IsDraft201909 => false;
    public virtual bool IsFormatAssertion => false;
}
