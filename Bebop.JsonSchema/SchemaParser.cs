using System.Collections.Frozen;

namespace Bebop.JsonSchema;

internal static class SchemaParser
{
    public static JsonSchema ParseSchema(JsonElement element, SchemaRegistry repo, Uri? retrievalUri)
    {
        var anchors = new Anchors();

        var isAnon = false;
        var id = _ParseId(element);
        if (id is null)
        {
            isAnon = retrievalUri is null;
            id = retrievalUri ?? repo.MakeRandomUri();
        }
        // todo: Assert $schema is compatible

        Assume
            .That(id.IsAbsoluteUri)
            .OtherwiseThrow(() => new InvalidSchemaException("Schema Root-ID must be an absolute URI.", element));

        var js = new JsonSchema(id, repo, anchors, isAnon);
        js.RootAssertion = _ParseAssertions(element, anchors, js, js, JsonPointer.Root);
        _AddMetaInfo(js, element);
        js.Path = JsonPointer.Root;

        return js;
    }

    internal static JsonSchema ParseSchema(JsonElement element, JsonSchema outerSchema, JsonSchema baseSchema, JsonPointer pathToBase)
    {
        var repo = outerSchema.Repository;
        var anchors = outerSchema.Anchors;

        var idLiteral = _ParseId(element);
        var id = idLiteral ?? repo.MakeRandomUri();
        var isNamed = idLiteral is not null;

        if (!id.IsAbsoluteUri)
        {
            var result = Uri.TryCreate(
                    new Uri(baseSchema.Id.GetLeftPart(UriPartial.Path), UriKind.Absolute),
                    id.OriginalString, 
                    out id);

            Assume.That(result).OtherwiseThrow(() => new InvalidSchemaException("Invalid schema ID.", element));
        }

        // Assert $schema is compatible


        if (isNamed)
        {
            anchors = new Anchors();
            pathToBase = JsonPointer.Root;
        }

        var js = new JsonSchema(id!, repo, anchors, !isNamed);

        if (isNamed)
        {
            // The inner schema defined a definite $id which means it must be resolvable
            outerSchema.Repository.AddSchema(js);
            baseSchema = js;
        }

        js.RootAssertion = _ParseAssertions(element, anchors, js, baseSchema, pathToBase);
        _AddMetaInfo(js, element);
        
        js.Path = pathToBase;
        js.Anchors.AddAnonymousAnchor(pathToBase, js);
        js.Repository.AddSchema(js);

        return js;
    }

    private static Assertion _ParseAssertions(
        JsonElement element,
        Anchors anchors,
        JsonSchema outerSchema, 
        JsonSchema baseSchema,
        JsonPointer pathToBase)
    {
        return element.ValueKind switch
        {
            JsonValueKind.False => NoneTypeAssertion.Instance,
            JsonValueKind.True => AnyTypeAssertion.Instance,
            JsonValueKind.Object => _ParseAssertionsCore(element, anchors, outerSchema, baseSchema, pathToBase),

            _ => throw new InvalidSchemaException("Schema must be an object, true, or false.", element)
        };
    }

    private static Assertion _ParseAssertionsCore(
        JsonElement element, 
        Anchors anchors,
        JsonSchema outerSchema,
        JsonSchema baseSchema,
        JsonPointer pathToBase)
    {
        var assertions = new List<Assertion>();


        foreach (var property in element.EnumerateObject())
        {
            var propertyName = property.Name;
            var propertyValue = property.Value;

            var ptb = pathToBase.AppendPropertyName(propertyName);

            var assertion = propertyName switch
            {
                "$id" or "$schema" or "$comment" or "description" or "title" => null, // ignored for validation purposes

                "type" => propertyValue switch
                {
                    { ValueKind: JsonValueKind.String } => _GetAssertionForType(propertyValue.ExpectString(), element),
                    { ValueKind: JsonValueKind.Array } => new OrCombinedAssertion(
                        propertyValue
                            .EnumerateArray()
                            .Select(e => _GetAssertionForType(e.ExpectString(), element))
                            .ToArray()
                    ),
                    _ => throw new InvalidSchemaException("Illegal type property.", element)
                },
                "const" => new ConstAssertion(propertyValue),

                "not" => new NotAssertion(ParseSchema(propertyValue, outerSchema, baseSchema, ptb)),
                "allOf" => new AllOfAssertion(propertyValue.ExpectArray().Select((e, i) => ParseSchema(e, outerSchema, baseSchema, ptb.AppendIndex(i))).ToArray()),
                "anyOf" => new AnyOfAssertion(propertyValue.ExpectArray().Select((e, i) => ParseSchema(e, outerSchema, baseSchema, ptb.AppendIndex(i))).ToArray()),
                "oneOf" => new OneOfAssertion(propertyValue.ExpectArray().Select((e, i) => ParseSchema(e, outerSchema, baseSchema, ptb.AppendIndex(i))).ToArray()),
                "enum" => new EnumAssertion(propertyValue.ExpectArray().ToArray()),
                "$ref" => _Resolve(propertyValue.ExpectUri(), outerSchema, baseSchema, false),
                "$dynamicRef" => _Resolve(propertyValue.ExpectUri(), outerSchema, baseSchema, true),
                "$anchor" => _ParseAnchor(propertyValue.ExpectString(), anchors, outerSchema, false),
                "$dynamicAnchor" => _ParseAnchor(propertyValue.ExpectString(), anchors, outerSchema, true),

                // String specific
                "minLength" => new MinLengthAssertion(property.ExpectNonNegativeCount()),
                "maxLength" => new MaxLengthAssertion(property.ExpectNonNegativeCount()),
                "pattern" => new PatternAssertion(property.ExpectString()),

                // Array specific
                "minItems" => new MinItemsAssertion(property.ExpectNonNegativeCount()),
                "maxItems" => new MaxItemsAssertion(property.ExpectNonNegativeCount()),
                "uniqueItems" => property.ExpectBoolean()
                    ? UniqueItemsAssertion.Instance
                    : null,
                "contains" => new ContainsAssertion(ParseSchema(propertyValue, outerSchema, baseSchema, ptb), _GetMinContains(element), _GetMaxContains(element)),
                "maxContains" or "minContains" => null, // Fetched by 'contains' already.
                "items" => new ItemsAssertion(ParseSchema(propertyValue, outerSchema, baseSchema, ptb)),
                "prefixItems" => new PrefixItemsAssertion(propertyValue.ExpectArray().Select((e, i) => ParseSchema(e, outerSchema, baseSchema, ptb.AppendIndex(i))).ToArray()),
                "unevaluatedItems" => new UnevaluatedItemsAssertion(ParseSchema(propertyValue, outerSchema, baseSchema, ptb)),

                // Number specific
                "minimum" => new MinimumPropertyAssertion(property.ExpectNumber()),
                "maximum" => new MaximumPropertyAssertion(property.ExpectNumber()),
                "exclusiveMinimum" => new ExclusiveMinimumPropertyAssertion(property.ExpectNumber()),
                "exclusiveMaximum" => new ExclusiveMaximumPropertyAssertion(property.ExpectNumber()),
                "multipleOf" => new MultipleOfAssertion(property.ExpectNumber()),

                // Object specific
                "minProperties" => new MinPropertiesAssertion(property.ExpectNonNegativeCount()),
                "maxProperties" => new MaxPropertiesPropertyAssertion(property.ExpectNonNegativeCount()),
                "required" => new RequiredAssertion(
                    property
                        .ExpectArray()
                        .Select(e => e.ExpectString())
                        .ToArray()
                ),
                "properties" => new PropertiesAssertion(
                    propertyValue.ExpectObject().EnumerateObject().ToFrozenDictionary(
                        x => x.Name,
                        x => ParseSchema(x.Value, outerSchema, baseSchema, ptb.AppendPropertyName(x.Name)))),
                "propertyNames" => new PropertyNamesAssertion(ParseSchema(propertyValue, outerSchema, baseSchema, ptb)),
                "patternProperties" => new PatternPropertiesAssertion(
                    propertyValue.ExpectObject().EnumerateObject().ToFrozenDictionary(
                        x => x.Name,
                        x => ParseSchema(x.Value, outerSchema, baseSchema, ptb))),
                "dependentRequired" => new DependentRequiredAssertion(
                    property.ExpectObject().EnumerateObject().ToFrozenDictionary(
                    x => x.Name,
                    x => x.Value.ExpectArray().Select(
                        a => a.ExpectString()).ToArray())),
                "additionalProperties" => new AdditionalPropertiesAssertion(ParseSchema(propertyValue, outerSchema, baseSchema, ptb)),
                "unevaluatedProperties" => new UnevaluatedPropertiesAssertion(ParseSchema(propertyValue, outerSchema, baseSchema, ptb)),
                "default" => null,

                // Conditional
                "if" => new ConditionalAssertion(
                    ParseSchema(propertyValue, outerSchema, baseSchema, ptb),
                    _GetThen(element, outerSchema, baseSchema, pathToBase),
                    _GetElse(element, outerSchema, baseSchema, pathToBase)),

                "then" or "else" => element.TryGetProperty("if", out _) 
                    ? null 
                    : _ParseSubSchema(propertyValue, outerSchema, baseSchema, ptb),

                // Content
                "contentSchema"or "contentEncoding" or "contentMediaType" => null,

                // DependentSchema
                "dependentSchemas" => new DependentSchemaAssertion(
                    propertyValue.ExpectObject().EnumerateObject().ToDictionary(
                        x => x.Name,
                        x => ParseSchema(x.Value, outerSchema, baseSchema, ptb.AppendPropertyName(x.Name)))),

                // Format
                "format" => new FormatAssertion(Formats.Get(propertyValue.ExpectString())),

                "dependencies" => null,
                // Vocabulary
                //"$vocabulary" => _ParseVocabulary(propertyValue.ExpectObject(), validVocabularies),

                _ => _ParseSubSchema(propertyValue, outerSchema, baseSchema, ptb),
            };

            if (assertion is not null)
            {
                assertions.Add(assertion);
            }
        }

        var items = assertions.OfType<ItemsAssertion>().FirstOrDefault();
        var prefixItems = assertions.OfType<PrefixItemsAssertion>().FirstOrDefault();

        if (items is not null && prefixItems is not null)
        {
            var x = new CombinedItemsPrefixItemsAssertion(prefixItems.Schemas, items.Schema);
            
            assertions.Remove(items);
            assertions.Remove(prefixItems);
            assertions.Add(x);
        }

        var finalAssertions = assertions
            .OrderBy(x => x.Order)
            .ToArray();

        return AndCombinedAssertion.From(finalAssertions);
    }

    private static Assertion? _ParseVocabulary(JsonElement propertyValue, HashSet<Uri> validVocabularies)
    {
        foreach (var vocab in propertyValue.ExpectObject().EnumerateObject())
        {
            var vocabUri = new Uri(vocab.Name, UriKind.Absolute);
            var isRequired = vocab.Value.ExpectBoolean();

            if (!isRequired)
            {
                validVocabularies.Remove(vocabUri);
            }
        }

        return null;
    }

    private static void _AddMetaInfo(JsonSchema js, JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return;

        if (element.TryGetProperty("$comment", out var comment))
        {
            js.Comment = comment.ExpectString();
        }

        if (element.TryGetProperty("description", out var description))
        {
            js.Description = description.ExpectString();
        }
    }

    private static JsonSchema _GetThen(JsonElement element, JsonSchema outerSchema, JsonSchema baseSchema, JsonPointer pathToBase)
    {
        if (element.TryGetProperty("then", out var value))
            return ParseSchema(value, outerSchema, baseSchema, pathToBase.AppendPropertyName("then"));

        return JsonSchema.True;
    }

    private static JsonSchema _GetElse(JsonElement element, JsonSchema outerSchema, JsonSchema baseSchema, JsonPointer pathToBase)
    {
        if (element.TryGetProperty("else", out var value))
            return ParseSchema(value, outerSchema, baseSchema, pathToBase.AppendPropertyName("else"));
        
        return JsonSchema.True;
    }

    private static int? _GetMinContains(JsonElement element)
    {
        if (element.TryGetProperty("minContains", out var minContainsElement))
            return minContainsElement.ExpectNonNegativeNumber();

        return null;
    }

    private static int? _GetMaxContains(JsonElement element)
    {
        if (element.TryGetProperty("maxContains", out var maxContainsElement))
            return maxContainsElement.ExpectNonNegativeNumber();

        return null;
    }

    private static Assertion? _ParseAnchor(string anchor, Anchors anchors, JsonSchema outerSchema, bool isDynamic)
    {
        var anchorUri = JsonPointer.Parse(anchor);

        if (isDynamic)
            anchors.AddDynamicNamedAnchor(anchorUri, outerSchema);
        else
            anchors.AddNamedAnchor(anchorUri, outerSchema);

        return null;
    }

    private static Assertion? _ParseSubSchema(JsonElement element, JsonSchema outerSchema, JsonSchema baseSchema, JsonPointer pathToBase)
    {
        ParseSchema(element, outerSchema, baseSchema, pathToBase);
        return null;
    }

    private static Assertion _Resolve(Uri refUri, JsonSchema outerSchema, JsonSchema baseSchema, bool isDynamic)
    {
        var repo = outerSchema.Repository;

        if (refUri.IsAbsoluteUri)
        {
            var absUri = refUri.WithoutFragment();
            var hasFrag = refUri.GetFragment(out var frag);

            return !hasFrag
                ? new RefAssertion(absUri, repo)
                : isDynamic 
                    ? new DynamicRefWithPointerAssertion(absUri, repo, JsonPointer.ParseFromUriFragment(frag[1..]))
                    : new RefWithPointerAssertion(absUri, repo, JsonPointer.ParseFromUriFragment(frag[1..]));
        }
        else
        {
            var relUri = refUri.WithoutFragment();
            var hasFrag = refUri.GetFragment(out var frag);

            var baseSchemaId = outerSchema.IsAnonymous 
                ? baseSchema.Id 
                : outerSchema.Id;

            if (string.IsNullOrEmpty(relUri.OriginalString))
            {
                Assume.That(hasFrag).OtherwiseThrow(() => new InvalidSchemaException($"Invalid relative URI '{refUri}'"));
                // reference to current document via #
                if (frag.Equals("#", StringComparison.Ordinal))
                    return new RefAssertion(baseSchemaId, repo);

                return isDynamic
                    ? new DynamicRefWithPointerAssertion(baseSchemaId, repo, JsonPointer.ParseFromUriFragment(frag[1..]))
                    : new RefWithPointerAssertion(baseSchemaId, repo, JsonPointer.ParseFromUriFragment(frag[1..]));
            }

            if (!Uri.TryCreate(
                    new Uri(baseSchemaId.GetLeftPart(UriPartial.Path), UriKind.Absolute),
                    relUri,
                    out var absUri))
            {
                throw new InvalidSchemaException($"Invalid relative URI '{refUri}'");
            }

            return !hasFrag
                ? new RefAssertion(absUri, repo)
                : isDynamic 
                    ? new DynamicRefWithPointerAssertion(absUri, repo, JsonPointer.ParseFromUriFragment(frag[1..])) 
                    : new RefWithPointerAssertion(absUri, repo, JsonPointer.ParseFromUriFragment(frag[1..]));
        }
    }

    private static Uri? _ParseId(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        if (!element.TryGetProperty("$id", out var idElement))
            return null;

        var uriString = idElement switch
        {
            { ValueKind: JsonValueKind.String } => idElement.ExpectString(),
            _ => throw new InvalidSchemaException("Invalid or missing $id property.", element)
        };

        return new Uri(uriString, UriKind.RelativeOrAbsolute);
    }

    private static Assertion _GetAssertionForType(string type, JsonElement value)
    {
        return type switch
        {
            "string" => StringTypeAssertion.Instance,
            "number" => NumberTypeAssertion.Instance,
            "integer" => IntegerTypeAssertion.Instance,
            "boolean" => BooleanTypeAssertion.Instance,
            "object" => ObjectTypeAssertion.Instance,
            "array" => ArrayTypeAssertion.Instance,
            "null" => NullTypeAssertion.Instance,
            _ => throw new InvalidSchemaException("Unknown type-keyword value.", value)
        };
    }
}