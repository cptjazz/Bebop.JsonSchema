using System.Collections.Frozen;

namespace Bebop.JsonSchema;

internal sealed class Dialect_Draft201909 : Dialect
{
    internal readonly static Dialect_Draft201909 Instance = new();

    public IReadOnlySet<string> CoreKeywords { get; } = new HashSet<string>
    {
        "$id",
        "$schema",
        "$ref",
        "$anchor",
        "$recursiveRef",
        "$recursiveAnchor",
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
        "additionalItems",
        "contains",
        "properties",
        "patternProperties",
        "additionalProperties",
        "dependentSchemas",
        "propertyNames",
        "unevaluatedItems",
        "unevaluatedProperties",
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

    public Dialect_Draft201909()
    {
        SupportedKeywords = CoreKeywords
            .Concat(ApplicatorKeywords)
            .Concat(ContentKeywords)
            .Concat(FormatAnnotationKeywords)
            .Concat(MetaDataKeywords)
            .Concat(ValidationKeywords)
            .ToFrozenSet();
    }

    override public IReadOnlySet<string> SupportedKeywords { get; }

    public override bool IsDraft201909 => true;

    override public IReadOnlySet<string> GetKeywordSet(Uri vocabularyUri)
    {
        return vocabularyUri.AbsoluteUri switch
        {
            Vocabularies_Draft201909.Core => CoreKeywords,
            Vocabularies_Draft201909.Applicator => ApplicatorKeywords,
            Vocabularies_Draft201909.Content => ContentKeywords,
            Vocabularies_Draft201909.FormatAnnotation => FormatAnnotationKeywords,
            Vocabularies_Draft201909.Metadata => MetaDataKeywords,
            Vocabularies_Draft201909.Validation => ValidationKeywords,
            _ => throw new InvalidOperationException($"Unable to get keyword set for the given URI: '{vocabularyUri.AbsoluteUri}'"),
        };
    }
}
