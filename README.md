# Bebop.JsonSchema

[![Build and Test](https://github.com/cptjazz/Bebop.JsonSchema/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/cptjazz/Bebop.JsonSchema/actions/workflows/build-and-test.yml)
[![Coverage Status](https://coveralls.io/repos/github/cptjazz/Bebop.JsonSchema/badge.svg?branch=main)](https://coveralls.io/github/cptjazz/Bebop.JsonSchema?branch=main)
[![NuGet](https://img.shields.io/nuget/v/Bebop.JsonSchema.svg)](https://www.nuget.org/packages/Bebop.JsonSchema/)

A high-performance JSON Schema validator for .NET, built on top of `System.Text.Json`.

## Supported JSON Schema Versions

| Version | Status |
|---|---|
| Draft 2020-12 | ✅ Fully supported (default) |
| Draft 2019-09 | ✅ Fully supported |
| Draft-07 | ❌ Not supported |
| Draft-06 | ❌ Not supported |
| Draft-04 | ❌ Not supported |

When no `$schema` is specified, Draft 2020-12 is assumed.
Custom meta-schemas are supported — define your own vocabularies and dialects on top of Draft 2020-12 or 2019-09.

## Features

- **Zero-allocation validation path** — designed with performance in mind; minimal allocations on the hot path
- **Built on `System.Text.Json`** — no third-party JSON parser dependencies
- **Async-first API** — schema creation, preparation, and validation are all async
- **`$ref` and `$dynamicRef` resolution** — local, HTTP-resolving, and custom schema registries
- **Format validation** — built-in validators for `email`, `ipv4`, `ipv6`, `uri`, `uuid`, `date-time`, `date`, `time`, `duration`, and more. In Draft 2020-12 and 2019-09, `format` is annotation-only by default and becomes an assertion when the `format-assertion` vocabulary is active.
- **Comprehensive keyword support** — type, enum, const, allOf/anyOf/oneOf/not, if/then/else, properties, patternProperties, additionalProperties, items, prefixItems, contains, uniqueItems, unevaluatedProperties, unevaluatedItems, dependentSchemas, dependentRequired, dependencies, and all validation keywords

## Targets

| Framework | Supported |
|-----------|-----------|
| .NET 8    | ✅        |
| .NET 10   | ✅        |

## Installation

Install via NuGet:

```bash
dotnet add package Bebop.JsonSchema
```

Or visit the [NuGet package page](https://www.nuget.org/packages/Bebop.JsonSchema/).

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

## Performance

Benchmarks validate a realistic **Person** object (nested objects, arrays, `$ref`, `$defs`,
`pattern`, `format`, `additionalProperties`, `if`/`then`) against a ~230-line Draft 2020-12 schema.
Compared against [JsonSchema.Net](https://www.nuget.org/packages/JsonSchema.Net) v8.0.4.

``` ini
BenchmarkDotNet v0.15.8, Windows 11
Intel Core i7-6560U CPU 2.20GHz (Skylake), 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.103
```

| Method | Runtime | Mean | Allocated |
|---|---|---:|---:|
| **Bebop.JsonSchema** | .NET 10.0 | **42.3 μs** | **31.2 KB** |
| JsonSchema.Net | .NET 10.0 | 109.4 μs | 78.0 KB |
| **Bebop.JsonSchema** | .NET 9.0 | **40.6 μs** | **31.5 KB** |
| JsonSchema.Net | .NET 9.0 | 110.6 μs | 78.9 KB |

**~2.6× faster** and **~60 % less memory** than JsonSchema.Net on the same workload.

> Reproduce locally: `dotnet run --project Benchmarks.Bebop.JsonSchema -c Release`

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
