using System.Collections.Frozen;

namespace Bebop.JsonSchema;

internal sealed class Dialect_Draft202012 : Dialect
{
    internal readonly static Dialect_Draft202012 Instance = new();

    public IReadOnlySet<string> CoreKeywords { get; } = new HashSet<string>
    {
        "$id",
        "$schema",
        "$ref",
        "$anchor",
        "$dynamicRef",
        "$dynamicAnchor",
        "type",
        "enum",
        "const",
        "$vocabulary",
        "$comment",
        "$defs",
    };

    public IReadOnlySet<string> ApplicatorKeywords { get; } = new HashSet<string>
    { 
        "if",
        "then",
        "else",
        "allOf",
        "anyOf",
        "oneOf",
        "not",
        "items",
        "prefixItems",
        "contains",
        "additionalItems",
        "properties",
        "patternProperties",
        "additionalProperties",
        "dependentSchemas",
        "propertyNames",
    };

    public IReadOnlySet<string> ContentKeywords { get; } = new HashSet<string>
    {
        "contentMediaType",
        "contentEncoding",
        "contentSchema",
    };

    public IReadOnlySet<string> FormatAnnotationKeywords { get; } = new HashSet<string>
    {
        "format",
    };

    public IReadOnlySet<string> MetaDataKeywords { get; } = new HashSet<string>
    {
        "title",
        "description",
        "default",
        "deprecated",
        "readOnly",
        "writeOnly",
        "examples",
    };

    public IReadOnlySet<string> UnevaluatedKeywords { get; } = new HashSet<string>
    {
        "unevaluatedItems",
        "unevaluatedProperties",
    };

    public IReadOnlySet<string> ValidationKeywords { get; } = new HashSet<string>
    {
        "type",
        "const",
        "enum",
        "multipleOf",
        "maximum",
        "exclusiveMaximum",
        "minimum",
        "exclusiveMinimum",
        "maxLength",
        "minLength",
        "pattern",
        "maxItems",
        "minItems",
        "uniqueItems",
        "maxContains",
        "minContains",
        "maxProperties",
        "minProperties",
        "required",
        "dependentRequired",
    };

    public Dialect_Draft202012()
    {
        SupportedKeywords = CoreKeywords
            .Concat(ApplicatorKeywords)
            .Concat(ContentKeywords)
            .Concat(FormatAnnotationKeywords)
            .Concat(MetaDataKeywords)
            .Concat(UnevaluatedKeywords)
            .Concat(ValidationKeywords)
            .ToFrozenSet();
    }

    override public IReadOnlySet<string> SupportedKeywords { get; }

    public override bool IsDraft202012 => true;

    override public IReadOnlySet<string> GetKeywordSet(Uri vocabularyUri)
    {
        return vocabularyUri.AbsoluteUri switch
        {
            Vocabularies_Draft202012.Core => CoreKeywords,
            Vocabularies_Draft202012.Applicator => ApplicatorKeywords,
            Vocabularies_Draft202012.Content => ContentKeywords,
            Vocabularies_Draft202012.FormatAnnotation => FormatAnnotationKeywords,
            Vocabularies_Draft202012.Metadata => MetaDataKeywords,
            Vocabularies_Draft202012.Unevaluated => UnevaluatedKeywords,
            Vocabularies_Draft202012.Validation => ValidationKeywords,
            _ => throw new InvalidOperationException($"Unable to get keyword set for the given URI: '{vocabularyUri.AbsoluteUri}'"),
        };
    }
}