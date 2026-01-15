using System.Collections.Frozen;

namespace Bebop.JsonSchema;

internal sealed class CustomDialect : Dialect
{
    private readonly Dialect _baseDialect;

    public CustomDialect(Dialect baseDialect, JsonSchema schema)
    {
        _baseDialect = baseDialect;
        var keywords = new HashSet<string>();
        
        if (schema.Vocabulary is not null)
        {
            foreach (var vocabUri in schema.Vocabulary)
            {
                var kws = GetKeywordSet(vocabUri);
                keywords.UnionWith(kws);
            }
        }

        SupportedKeywords = keywords.ToFrozenSet();
    }

    override public IReadOnlySet<string> SupportedKeywords { get; }

    public override IReadOnlySet<string> GetKeywordSet(Uri vocabularyUri)
    {
        return _baseDialect.GetKeywordSet(vocabularyUri);
    }
}
