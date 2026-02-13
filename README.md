# Bebop.JsonSchema

A high-performance JSON Schema validator for .NET, built on top of `System.Text.Json`.

## Features

- **JSON Schema Draft 2020-12** — full support for the latest JSON Schema specification
- **JSON Schema Draft 2019-09** — support for the previous draft
- **Custom meta-schemas** — define and use your own vocabularies and dialects
- **Zero-allocation validation path** — designed with performance in mind; minimal allocations on the hot path
- **Built on `System.Text.Json`** — no third-party JSON parser dependencies
- **Async-first API** — schema creation, preparation, and validation are all async
- **`$ref` and `$dynamicRef` resolution** — local, HTTP-resolving, and custom schema registries
- **Format validation** — built-in validators for `email`, `ipv4`, `ipv6`, `uri`, `uuid`, `date-time`, `date`, `time`, `duration`, and more
- **Comprehensive keyword support** — type, enum, const, allOf/anyOf/oneOf/not, if/then/else, properties, patternProperties, additionalProperties, items, prefixItems, contains, uniqueItems, unevaluatedProperties, unevaluatedItems, dependentSchemas, dependentRequired, and all validation keywords

## Targets

| Framework | Supported |
|-----------|-----------|
| .NET 8    | ✅        |
| .NET 10   | ✅        |

## Getting Started

### Create and validate against a schema

```csharp
using Bebop.JsonSchema;
using System.Text.Json;

// Define a schema
var schema = await JsonSchema.Create("""
    {
      "$schema": "https://json-schema.org/draft/2020-12/schema",
      "type": "object",
      "required": ["name", "age"],
      "properties": {
        "name": { "type": "string", "minLength": 1 },
        "age":  { "type": "integer", "minimum": 0 }
      }
    }
    """);

// Prepare the schema (resolves $ref, etc.)
await schema.Prepare();

// Validate a JSON document
using var doc = JsonDocument.Parse("""{ "name": "Alice", "age": 30 }""");

var errors = new ErrorCollection();
bool isValid = await schema.Validate(doc, errors);
// isValid == true
```

### Validate from a `JsonElement`

```csharp
using var doc = JsonDocument.Parse("""{ "name": "", "age": -1 }""");

var errors = new ErrorCollection();
bool isValid = await schema.Validate(doc.RootElement, errors);
// isValid == false
```

### Use a remote-resolving schema registry

If your schema references external schemas via `$ref`, use a resolving registry that fetches them over HTTP:

```csharp
var registry = SchemaRegistry.Resolving();
var schema = await JsonSchema.Create(schemaDocument, registry);
await schema.Prepare();
```

### Use a custom schema resolver

Implement `ISchemaResolver` to control how referenced schemas are loaded (e.g. from a database, embedded resources, or a local file system):

```csharp
public class MyResolver : ISchemaResolver
{
    public async ValueTask<JsonElement?> Resolve(Uri id)
    {
        // Load the schema JSON by URI and return the root element,
        // or return null if not found.
    }
}

var registry = SchemaRegistry.Custom(new MyResolver());
var schema = await JsonSchema.Create(schemaDocument, registry);
await schema.Prepare();
```

## Schema Registries

| Registry | Description |
|---|---|
| `SchemaRegistry.Local()` | In-memory only. No external resolution. Default when none is specified. |
| `SchemaRegistry.Resolving()` | Fetches remote schemas over HTTP/HTTPS on demand. |
| `SchemaRegistry.Custom(resolver)` | Delegates resolution to your `ISchemaResolver` implementation. |

## Built-in Format Validators

The following `format` values are validated when the format vocabulary is active:

`email` · `idn-email` · `ipv4` · `ipv6` · `uri` · `uuid` · `date-time` · `date` · `time` · `duration`

## Project Structure

```
Bebop.JsonSchema/              # Core library
Tests.Bebop.JsonSchema/        # xUnit test suite (JSON Schema Test Suite)
Benchmarks.Bebop.JsonSchema/   # BenchmarkDotNet benchmarks
```

## Building

```bash
dotnet build
```

## Running Tests

```bash
dotnet test
```

## Running Benchmarks

```bash
dotnet run --project Benchmarks.Bebop.JsonSchema -c Release
```

## License

See [LICENSE](LICENSE) for details.