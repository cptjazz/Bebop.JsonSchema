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
                if (!_baseDialect.TryGetKeywordSet(vocabUri, out var kws))
                    continue;

                keywords.UnionWith(kws);

                if (vocabUri.AbsoluteUri == Vocabularies_Draft202012.FormatAssertion)
                {
                    IsFormatAssertion = true;
                }
            }
        }

        SupportedKeywords = keywords.ToFrozenSet();
    }

    override public IReadOnlySet<string> SupportedKeywords { get; }

    public override bool IsFormatAssertion { get; }

    public override IReadOnlySet<string> GetKeywordSet(Uri vocabularyUri)
    {
        return _baseDialect.GetKeywordSet(vocabularyUri);
    }

    public override bool TryGetKeywordSet(Uri vocabularyUri, out IReadOnlySet<string> keywordSet)
    {
        return _baseDialect.TryGetKeywordSet(vocabularyUri, out keywordSet);
    }
}
