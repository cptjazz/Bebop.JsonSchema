using System.Collections.Frozen;

namespace Bebop.JsonSchema;

internal static class SchemaParser
{
    public static async ValueTask<JsonSchema> ParseSchema(JsonElement element, SchemaRegistry repo, Uri? retrievalUri)
    {
        var anchors = new Anchors();

        var isAnon = false;
        var id = _ParseId(element);

        if (id is null)
        {
            isAnon = retrievalUri is null;
            id = retrievalUri ?? repo.MakeRandomUri();
        }
        else
        {
            // If ID is not absolute, base it on the retrieval URI if available.
            if (!id.IsAbsoluteUri && retrievalUri is not null)
            {
                id = new Uri(retrievalUri, id);
            }
        }

        Assume
            .That(id.IsAbsoluteUri)
            .OtherwiseThrow(() => new InvalidSchemaException("Schema Root-ID must be an absolute URI.", element));


        var schemaUri = element.ValueKind == JsonValueKind.Object && element.TryGetProperty("$schema", out var schemaElement)
            ? new Uri(schemaElement.ExpectString(), UriKind.Absolute)
            : Dialect.DefaultDialectUri;

        await SyncContext.Drop();
        
        var dialect = Dialect.Get(schemaUri) ??
                      Dialect.FromSchema(await repo.GetSchema(schemaUri) ?? 
                                         throw new InvalidSchemaException("Unable to find custom meta-schema"));

        var js = new JsonSchema(id, repo, anchors, isAnon, dialect);

        anchors.AddAnonymousAnchor(JsonPointer.Hash, js);

        js.RootAssertion = await _ParseAssertions(element, anchors, js, js, JsonPointer.Root, dialect);
        _AddMetaInfo(js, element);
        js.Path = JsonPointer.Root;

        return js;
    }

    internal static async ValueTask<JsonSchema> ParseSchema(
        JsonElement element, 
        JsonSchema outerSchema, 
        JsonSchema baseSchema, 
        JsonPointer pathToBase,
        Dialect baseDialect,
        bool ignoreId = false)
    {
        var repo = outerSchema.Repository;
        var anchors = outerSchema.Anchors;

        var idLiteral = ignoreId ? null : _ParseId(element);
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

        var dialect = baseDialect;
        await SyncContext.Drop();

        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("$schema", out var schemaElement))
        {
            var schemaUri = new Uri(schemaElement.ExpectString(), UriKind.Absolute);

            dialect = Dialect.Get(schemaUri) ??
                          Dialect.FromSchema(await repo.GetSchema(schemaUri) ??
                                             throw new InvalidSchemaException("Unable to find custom meta-schema"));
        }
        
        if (isNamed)
        {
            anchors = new Anchors();
            pathToBase = JsonPointer.Root;
        }

        var js = new JsonSchema(id!, repo, anchors, !isNamed, dialect);

        if (isNamed)
        {
            // The inner schema defined a definite $id which means it must be resolvable
            outerSchema.Repository.AddSchema(js);
            baseSchema = js;

            anchors.AddAnonymousAnchor(JsonPointer.Hash, js);
        }

        js.RootAssertion = await _ParseAssertions(element, anchors, js, baseSchema, pathToBase, dialect);
        _AddMetaInfo(js, element);
        
        js.Path = pathToBase;
        js.Anchors.AddAnonymousAnchor(pathToBase, js);
        js.Repository.AddSchema(js);

        return js;
    }

    private static ValueTask<Assertion> _ParseAssertions(
        JsonElement element,
        Anchors anchors,
        JsonSchema outerSchema, 
        JsonSchema baseSchema,
        JsonPointer pathToBase,
        Dialect dialect)
    {
        return element.ValueKind switch
        {
            JsonValueKind.False => ValueTask.FromResult<Assertion>(NoneTypeAssertion.Instance),
            JsonValueKind.True => ValueTask.FromResult<Assertion>(AnyTypeAssertion.Instance),
            JsonValueKind.Object => _ParseAssertionsCore(element, anchors, outerSchema, baseSchema, pathToBase, dialect),

            _ => ValueTask.FromException<Assertion>(new InvalidSchemaException("Schema must be an object, true, or false.", element))
        };
    }

    private static async ValueTask<Assertion> _ParseAssertionsCore(
        JsonElement element, 
        Anchors anchors,
        JsonSchema outerSchema,
        JsonSchema baseSchema,
        JsonPointer pathToBase,
        Dialect dialect)
    {
        var assertions = new List<Assertion>();
        await SyncContext.Drop();

        foreach (var property in element.EnumerateObject())
        {
            var propertyName = property.Name;
            var propertyValue = property.Value;

            var ptb = pathToBase.AppendPropertyName(propertyName);
            var isKeyword = dialect.SupportedKeywords.Contains(propertyName);

            var assertion = isKeyword
                ? propertyName switch
                {
                    "$id" or "$schema" or "$comment" or "description" or "title"
                        or "deprecated" or "readOnly" or "writeOnly" => null, // ignored for validation purposes

                    "$defs" => await _ParseDefs(propertyValue, outerSchema, baseSchema, ptb, dialect),

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

                    "not" => new NotAssertion(await ParseSchema(propertyValue, outerSchema, baseSchema, ptb, dialect)),
                    "allOf" => new AllOfAssertion(await propertyValue.ExpectArray().SelectArray((e, i) => ParseSchema(e, outerSchema, baseSchema, ptb.AppendIndex(i), dialect))),
                    "anyOf" => new AnyOfAssertion(await propertyValue.ExpectArray().SelectArray((e, i) => ParseSchema(e, outerSchema, baseSchema, ptb.AppendIndex(i), dialect))),
                    "oneOf" => new OneOfAssertion(await propertyValue.ExpectArray().SelectArray((e, i) => ParseSchema(e, outerSchema, baseSchema, ptb.AppendIndex(i), dialect))),
                    "enum" => new EnumAssertion(propertyValue.ExpectArray().ToArray()),
                    "$ref" => _Resolve(propertyValue.ExpectUri(), outerSchema, baseSchema, false),
                    "$dynamicRef" or "$recursiveRef" => _Resolve(propertyValue.ExpectUri(), outerSchema, baseSchema, true),
                    "$anchor" => _ParseAnchor(propertyValue.ExpectString(), anchors, outerSchema, false),
                    "$dynamicAnchor" => _ParseAnchor(propertyValue.ExpectString(), anchors, outerSchema, true),
                    "$recursiveAnchor" => propertyValue.GetBoolean()
                        ? _ParseAnchor("#", anchors, outerSchema, true)
                        : null,

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
                    "contains" => new ContainsAssertion(await ParseSchema(propertyValue, outerSchema, baseSchema, ptb, dialect), _GetMinContains(element), _GetMaxContains(element)),
                    "maxContains" or "minContains" => null, // Fetched by 'contains' already.
                    "items" => await _ItemsAssertion(outerSchema, baseSchema, dialect, propertyValue, ptb), // items-array (2019-09) == prefixItems (2020-12)
                    "additionalItems" => new AdditionalItemsAssertion(await ParseSchema(propertyValue, outerSchema, baseSchema, ptb, dialect)), // additionalItems (2019-09) == items (2020-12)
                    "prefixItems" => new PrefixItemsAssertion(await propertyValue.ExpectArray().SelectArray((e, i) => ParseSchema(e, outerSchema, baseSchema, ptb.AppendIndex(i), dialect))),
                    "unevaluatedItems" => new UnevaluatedItemsAssertion(await ParseSchema(propertyValue, outerSchema, baseSchema, ptb, dialect)),

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
                        await propertyValue.ExpectObject().EnumerateObject().ToFrozenDictionary(
                            x => x.Name,
                            x => ParseSchema(x.Value, outerSchema, baseSchema, ptb.AppendPropertyName(x.Name), dialect))),
                    "propertyNames" => new PropertyNamesAssertion(await ParseSchema(propertyValue, outerSchema, baseSchema, ptb, dialect)),
                    "patternProperties" => new PatternPropertiesAssertion(
                        await propertyValue.ExpectObject().EnumerateObject().ToFrozenDictionary(
                            x => x.Name,
                            x => ParseSchema(x.Value, outerSchema, baseSchema, ptb, dialect))),
                    "dependentRequired" => new DependentRequiredAssertion(
                        property.ExpectObject().EnumerateObject().ToFrozenDictionary(
                        x => x.Name,
                        x => x.Value.ExpectArray().Select(
                            a => a.ExpectString()).ToArray())),
                    "additionalProperties" => new AdditionalPropertiesAssertion(await ParseSchema(propertyValue, outerSchema, baseSchema, ptb, dialect)),
                    "unevaluatedProperties" => new UnevaluatedPropertiesAssertion(await ParseSchema(propertyValue, outerSchema, baseSchema, ptb, dialect)),
                    "default" => null,

                    // Conditional
                    "if" => new ConditionalAssertion(
                        await ParseSchema(propertyValue, outerSchema, baseSchema, ptb, dialect),
                        await _GetThen(element, outerSchema, baseSchema, pathToBase, dialect),
                        await _GetElse(element, outerSchema, baseSchema, pathToBase, dialect)),

                    "then" or "else" => element.TryGetProperty("if", out _)
                        ? null
                        : await _ParseSubSchema(propertyValue, outerSchema, baseSchema, ptb, dialect),

                    // Content
                    "contentSchema" or "contentEncoding" or "contentMediaType" => null,

                    // DependentSchema
                    "dependentSchemas" => new DependentSchemaAssertion(
                        await propertyValue.ExpectObject().EnumerateObject().ToFrozenDictionary(
                            x => x.Name,
                            x => ParseSchema(x.Value, outerSchema, baseSchema, ptb.AppendPropertyName(x.Name), dialect))),

                    // Format
                    "format" => dialect.IsFormatAssertion
                        ? new FormatAssertion(Formats.Get(propertyValue.ExpectString()))
                        : null,

                    "dependencies" => await _ParseDependencies(propertyValue, outerSchema, baseSchema, ptb, dialect),

                    // Vocabulary
                    "$vocabulary" => _ParseVocabulary(propertyValue.ExpectObject(), baseSchema),

                    "examples" => await _ParseExamples(outerSchema, baseSchema, dialect, propertyValue, ptb),

                    _ => await _ParseSubSchema(propertyValue, outerSchema, baseSchema, ptb, dialect),
                }
            : await _ParseSubSchema(propertyValue, outerSchema, baseSchema, ptb, dialect, true);

            if (assertion is not null)
            {
                assertions.Add(assertion);
            }
        }

        if (dialect.IsDraft202012)
            _Blubb2020(assertions);

        if (dialect.IsDraft201909)
             _Blubb2019(assertions);

        var finalAssertions = assertions
            .OrderBy(x => x.Order)
            .ToArray();

        return AndCombinedAssertion.From(finalAssertions);
    }

    private static async Task<Assertion?> _ParseDefs(
        JsonElement propertyValue,
        JsonSchema outerSchema,
        JsonSchema baseSchema,
        JsonPointer ptb,
        Dialect dialect)
    {
        foreach (var def in propertyValue.ExpectObject().EnumerateObject())
        {
            await _ParseSubSchema(def.Value, outerSchema, baseSchema, ptb.AppendPropertyName(def.Name), dialect);
        }

        return null;
    }

    private static async Task<Assertion?> _ParseExamples(JsonSchema outerSchema, JsonSchema baseSchema, Dialect dialect, JsonElement propertyValue, JsonPointer ptb)
    {
        var examples = propertyValue.ExpectArray();
        _ = await examples.SelectArray((e, i) => _ParseSubSchema(e, outerSchema, baseSchema, ptb.AppendIndex(i), dialect));

        return null;
    }

    private static void _Blubb2020(List<Assertion> assertions)
    {
        var items = assertions.OfType<ItemsAssertion>().FirstOrDefault();
        var prefixItems = assertions.OfType<PrefixItemsAssertion>().FirstOrDefault();

        if (items is not null && prefixItems is not null)
        {
            var x = new CombinedItemsPrefixItemsAssertion(prefixItems.Schemas, items.Schema);

            assertions.Remove(items);
            assertions.Remove(prefixItems);
            assertions.Add(x);
        }
    }

    private static void _Blubb2019(List<Assertion> assertions)
    {
        var items = assertions.OfType<ItemsAssertion>().FirstOrDefault();
        var prefixItems = assertions.OfType<PrefixItemsAssertion>().FirstOrDefault();
        var additionalItems = assertions.OfType<AdditionalItemsAssertion>().FirstOrDefault();

        if (additionalItems is not null)
        {
            if (items is null)
            {
                if (prefixItems is not null)
                {
                    items = new ItemsAssertion(additionalItems.Schema);
                    assertions.Add(items);
                }
            } 

            assertions.Remove(additionalItems);
            additionalItems = null;
        }

        if (items is not null && prefixItems is not null)
        {
            var x = new CombinedItemsPrefixItemsAssertion(prefixItems.Schemas, items.Schema);

            assertions.Remove(items);
            assertions.Remove(prefixItems);
            assertions.Add(x);
        }
    }

    private static async ValueTask<Assertion> _ItemsAssertion(JsonSchema outerSchema, JsonSchema baseSchema, Dialect dialect, JsonElement propertyValue, JsonPointer ptb)
    {
        await SyncContext.Drop();

        return dialect.IsDraft201909
            ? propertyValue switch
            {
                { ValueKind: JsonValueKind.Array } => new PrefixItemsAssertion(await propertyValue.ExpectArray().SelectArray((e, i) => ParseSchema(e, outerSchema, baseSchema, ptb.AppendIndex(i), dialect))),
                _ => new ItemsAssertion(await ParseSchema(propertyValue, outerSchema, baseSchema, ptb, dialect))
            }
            : new ItemsAssertion(await ParseSchema(propertyValue, outerSchema, baseSchema, ptb, dialect));
    }

    private static Assertion? _ParseVocabulary(JsonElement propertyValue, JsonSchema baseSchema)
    {
        HashSet<Uri> validVocabularies = new();

        foreach (var vocab in propertyValue.ExpectObject().EnumerateObject())
        {
            var vocabUri = new Uri(vocab.Name, UriKind.Absolute);
            vocab.Value.ExpectBoolean();

            // All declared vocabularies are included regardless of the required flag.
            // The flag only indicates whether the implementation MUST support the vocabulary;
            // optional (false) vocabularies are still used when the implementation supports them.
            validVocabularies.Add(vocabUri);
        }

        baseSchema.Vocabulary = validVocabularies;

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

    private static ValueTask<JsonSchema> _GetThen(
        JsonElement element, 
        JsonSchema outerSchema, 
        JsonSchema baseSchema, 
        JsonPointer pathToBase,
        Dialect dialect)
    {
        if (element.TryGetProperty("then", out var value))
            return ParseSchema(value, outerSchema, baseSchema, pathToBase.AppendPropertyName("then"), dialect);

        return ValueTask.FromResult(JsonSchema.True);
    }

    private static ValueTask<JsonSchema> _GetElse(
        JsonElement element, 
        JsonSchema outerSchema, 
        JsonSchema baseSchema, 
        JsonPointer pathToBase,
        Dialect dialect)
    {
        if (element.TryGetProperty("else", out var value))
            return ParseSchema(value, outerSchema, baseSchema, pathToBase.AppendPropertyName("else"), dialect);
        
        return ValueTask.FromResult(JsonSchema.True);
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

    private static async ValueTask<Assertion?> _ParseDependencies(
        JsonElement propertyValue,
        JsonSchema outerSchema,
        JsonSchema baseSchema,
        JsonPointer ptb,
        Dialect dialect)
    {
        await SyncContext.Drop();

        var requiredEntries = new Dictionary<string, string[]>();
        var schemaEntries = new Dictionary<string, JsonSchema>();

        foreach (var entry in propertyValue.ExpectObject().EnumerateObject())
        {
            if (entry.Value.ValueKind == JsonValueKind.Array)
            {
                requiredEntries[entry.Name] = entry.Value.EnumerateArray()
                    .Select(e => e.ExpectString())
                    .ToArray();
            }
            else
            {
                schemaEntries[entry.Name] = await ParseSchema(
                    entry.Value, outerSchema, baseSchema,
                    ptb.AppendPropertyName(entry.Name), dialect);
            }
        }

        var assertions = new List<Assertion>(2);

        if (requiredEntries.Count > 0)
            assertions.Add(new DependentRequiredAssertion(requiredEntries.ToFrozenDictionary()));

        if (schemaEntries.Count > 0)
            assertions.Add(new DependentSchemaAssertion(schemaEntries.ToFrozenDictionary()));

        return assertions.Count switch
        {
            0 => null,
            _ => AndCombinedAssertion.From(assertions.ToArray()),
        };
    }

    private static async ValueTask<Assertion?> _ParseSubSchema(
        JsonElement element, 
        JsonSchema outerSchema, 
        JsonSchema baseSchema, 
        JsonPointer pathToBase,
        Dialect dialect,
        bool tolerateInvalidSchema = false)
    {
        if (tolerateInvalidSchema)
        {
            if (element.ValueKind != JsonValueKind.Object &&
                element.ValueKind != JsonValueKind.True &&
                element.ValueKind != JsonValueKind.False)
            {
                return null;
            }
        }

        await SyncContext.Drop();
        var s = await ParseSchema(element, outerSchema, baseSchema, pathToBase, dialect, ignoreId: tolerateInvalidSchema);
        baseSchema.Anchors.AddAnonymousAnchor(pathToBase, s);

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
                    return isDynamic
                        ? new DynamicRefWithPointerAssertion(baseSchemaId, repo, JsonPointer.Parse("#"))
                        : new RefAssertion(baseSchemaId, repo);

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

internal static class AsyncExtensions
{
    public static async ValueTask<T[]> SelectArray<T>(this JsonElement.ArrayEnumerator source, Func<JsonElement, int, ValueTask<T>> selector)
    {
        var list = new List<T>();
        var index = 0;

        foreach (var item in source)
        {
            var selected = await selector(item, index).ConfigureAwait(false);
            list.Add(selected);
            index++;
        }
        
        return list.ToArray();
    }

    public static async ValueTask<FrozenDictionary<TKey, TValue>> ToFrozenDictionary<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, ValueTask<TValue>> valueSelector)
        where TKey : notnull
    {
        var dict = new Dictionary<TKey, TValue>();
        foreach (var item in source)
        {
            var key = keySelector(item);
            var value = await valueSelector(item).ConfigureAwait(false);
            dict[key] = value;
        }
        return dict.ToFrozenDictionary();
    }
}

