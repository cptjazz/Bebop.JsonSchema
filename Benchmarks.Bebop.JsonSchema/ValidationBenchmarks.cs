using System.Text.Json;
using Bebop.JsonSchema;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class ValidationBenchmarks
{
    private JsonDocument _personTestData1;
    private JsonSchema _personSchema;
    private Json.Schema.JsonSchema _personSchemaJE;

    [GlobalSetup]
    public async Task Setup()
    {
        var personSchemaText = """
                               {
                                 "$schema": "https://json-schema.org/draft/2020-12/schema",
                                 "$id": "https://example.com/schemas/person.schema.json",
                                 "title": "Person",
                                 "type": "object",
                                 "additionalProperties": false,
                                 "required": ["id", "givenName", "familyName", "primaryEmail", "addresses"],
                                 "properties": {
                                   "id": {
                                     "type": "string",
                                     "description": "Unique identifier for the person",
                                     "pattern": "^[A-Za-z0-9_-]{8,32}$"
                                   },
                                   "givenName": {
                                     "type": "string",
                                     "minLength": 1,
                                     "maxLength": 100
                                   },
                                   "middleName": {
                                     "type": "string",
                                     "minLength": 1,
                                     "maxLength": 100
                                   },
                                   "familyName": {
                                     "type": "string",
                                     "minLength": 1,
                                     "maxLength": 100
                                   },
                                   "fullName": {
                                     "type": "string",
                                     "minLength": 1,
                                     "maxLength": 200
                                   },
                                   "gender": {
                                     "type": "string",
                                     "enum": ["male", "female", "non_binary", "other", "unspecified"]
                                   },
                                   "dateOfBirth": {
                                     "type": "string",
                                     "format": "date"
                                   },
                                   "nationalId": {
                                     "type": "string",
                                     "maxLength": 64
                                   },
                                   "primaryEmail": {
                                     "type": "string",
                                     "format": "email"
                                   },
                                   "emails": {
                                     "type": "array",
                                     "items": {
                                       "type": "object",
                                       "additionalProperties": false,
                                       "required": ["email"],
                                       "properties": {
                                         "email": {
                                           "type": "string",
                                           "format": "email"
                                         },
                                         "type": {
                                           "type": "string",
                                           "enum": ["personal", "work", "other"],
                                           "default": "other"
                                         },
                                         "primary": {
                                           "type": "boolean",
                                           "default": false
                                         }
                                       }
                                     },
                                     "uniqueItems": true
                                   },
                                   "phoneNumbers": {
                                     "type": "array",
                                     "items": {
                                       "$ref": "#/$defs/phoneNumber"
                                     },
                                     "uniqueItems": true
                                   },
                                   "preferredLanguage": {
                                     "type": "string",
                                     "pattern": "^[a-zA-Z]{2,3}(-[a-zA-Z]{2})?$",
                                     "description": "BCP-47 language tag (e.g. 'en', 'de-AT')"
                                   },
                                   "addresses": {
                                     "type": "array",
                                     "items": {
                                       "$ref": "#/$defs/address"
                                     },
                                     "minItems": 1,
                                     "uniqueItems": true
                                   },
                                   "defaultShippingAddressId": {
                                     "type": "string"
                                   },
                                   "defaultBillingAddressId": {
                                     "type": "string"
                                   },
                                   "metadata": {
                                     "type": "object",
                                     "description": "Arbitrary key/value pairs",
                                     "additionalProperties": {
                                       "type": ["string", "number", "boolean", "null"]
                                     }
                                   },
                                   "createdAt": {
                                     "type": "string",
                                     "format": "date-time"
                                   },
                                   "updatedAt": {
                                     "type": "string",
                                     "format": "date-time"
                                   },
                                   "active": {
                                     "type": "boolean",
                                     "default": true
                                   }
                                 },
                                 "$defs": {
                                   "address": {
                                     "type": "object",
                                     "title": "Address",
                                     "additionalProperties": false,
                                     "required": ["id", "line1", "city", "postalCode", "country"],
                                     "properties": {
                                       "id": {
                                         "type": "string",
                                         "pattern": "^[A-Za-z0-9_-]{4,32}$"
                                       },
                                       "label": {
                                         "type": "string",
                                         "description": "Friendly name, e.g. 'Home', 'Office'",
                                         "maxLength": 100
                                       },
                                       "line1": {
                                         "type": "string",
                                         "maxLength": 200
                                       },
                                       "line2": {
                                         "type": "string",
                                         "maxLength": 200
                                       },
                                       "city": {
                                         "type": "string",
                                         "maxLength": 100
                                       },
                                       "state": {
                                         "type": "string",
                                         "maxLength": 100
                                       },
                                       "postalCode": {
                                         "type": "string",
                                         "maxLength": 20
                                       },
                                       "country": {
                                         "type": "string",
                                         "pattern": "^[A-Z]{2}$",
                                         "description": "ISO 3166-1 alpha-2 country code"
                                       },
                                       "isBilling": {
                                         "type": "boolean",
                                         "default": false
                                       },
                                       "isShipping": {
                                         "type": "boolean",
                                         "default": false
                                       }
                                     }
                                   },
                                   "phoneNumber": {
                                     "type": "object",
                                     "title": "Phone Number",
                                     "additionalProperties": false,
                                     "required": ["number"],
                                     "properties": {
                                       "number": {
                                         "type": "string",
                                         "pattern": "^\\+?[0-9 ()\\-]{6,32}$"
                                       },
                                       "type": {
                                         "type": "string",
                                         "enum": ["mobile", "landline", "fax", "other"],
                                         "default": "mobile"
                                       },
                                       "preferred": {
                                         "type": "boolean",
                                         "default": false
                                       }
                                     }
                                   }
                                 },
                                 "allOf": [
                                   {
                                     "if": {
                                       "properties": {
                                         "emails": {
                                           "type": "array",
                                           "minItems": 1
                                         }
                                       }
                                     },
                                     "then": {
                                       "properties": {
                                         "primaryEmail": {
                                           "type": "string",
                                           "format": "email"
                                         }
                                       }
                                     }
                                   }
                                 ]
                               }

                               """;
        _personSchema = await JsonSchema.Create(personSchemaText);
        await _personSchema.Prepare();

        _personSchemaJE = Json.Schema.JsonSchema.FromText(personSchemaText);

        var personTestDataText1 = """
                                  {
                                    "id": "user_123456",
                                    "givenName": "Alice",
                                    "familyName": "Müller",
                                    "fullName": "Alice Müller",
                                    "gender": "female",
                                    "dateOfBirth": "1990-05-21",
                                    "primaryEmail": "alice.mueller@example.com",
                                    "emails": [
                                      {
                                        "email": "alice.mueller@example.com",
                                        "type": "personal",
                                        "primary": true
                                      },
                                      {
                                        "email": "alice@work-example.com",
                                        "type": "work",
                                        "primary": false
                                      }
                                    ],
                                    "phoneNumbers": [
                                      {
                                        "number": "+43 316 123456",
                                        "type": "landline",
                                        "preferred": false
                                      },
                                      {
                                        "number": "+43 660 9876543",
                                        "type": "mobile",
                                        "preferred": true
                                      }
                                    ],
                                    "preferredLanguage": "de-AT",
                                    "addresses": [
                                      {
                                        "id": "addr_home",
                                        "label": "Home",
                                        "line1": "Hauptplatz 1",
                                        "line2": "Stiege 2, Top 3",
                                        "city": "Graz",
                                        "state": "Steiermark",
                                        "postalCode": "8010",
                                        "country": "AT",
                                        "isBilling": true,
                                        "isShipping": true
                                      }
                                    ],
                                    "defaultBillingAddressId": "addr_home",
                                    "defaultShippingAddressId": "addr_home",
                                    "metadata": {
                                      "customer_segment": "gold",
                                      "newsletter_opt_in": true
                                    },
                                    "createdAt": "2024-01-10T09:30:00Z",
                                    "updatedAt": "2024-06-15T12:00:00Z",
                                    "active": true
                                  }
                                  """;

        _personTestData1 = JsonDocument.Parse(personTestDataText1);
    }

    [Benchmark]
    public async Task<bool> Validate_Person()
    {
        var errorCollection = new ErrorCollection();
        return await _personSchema.Validate(_personTestData1.RootElement, errorCollection);
    }

    [Benchmark]
    public bool Validate_Person_JE()
    {
        var r = _personSchemaJE.Evaluate(_personTestData1.RootElement);
        return r.IsValid;
    }
}