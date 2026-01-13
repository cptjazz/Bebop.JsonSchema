namespace Tests.MyJsonSchema;

public class Test_SchemaRefs
{
    [Fact]
    public void Test_RefWithFragment()
    {
        var schema1 = """
                      {
                        "$id": "https://example.com/schemas/address",
                        "type": "object",
                        "properties": {
                          "street_address": { "$anchor": "street_address", "type": "string" },
                          "city": { "type": "string" },
                          "state": { "type": "string" }
                        }, 
                        "required": ["street_address", "city", "state"]
                      }
                      """;

        var schema2 = """
                      {
                        "$id": "https://example.com/schemas/customer",
                        "type": "object",
                        "properties": {
                          "first_name": { "type": "string" },
                          "last_name": { "type": "string" },
                          "shipping_address": { "$ref": "/schemas/address" },
                          "billing_address": { "$ref": "/schemas/address" }
                        },
                        "required": ["first_name", "last_name", "shipping_address", "billing_address"]
                      }
                      """;

        var repo = SchemaRegistry.Local();

        var addressSchema = JsonSchema.Create(JsonDocument.Parse(schema1), repo);
        var customerSchema = JsonSchema.Create(JsonDocument.Parse(schema2), repo);

        var data = """
                   {
                     "first_name": "John",
                     "last_name": "Doe",
                     "shipping_address": {
                       "street_address": "123 Main St",
                       "city": "Anytown",
                       "state": "CA"
                     },
                     "billing_address": {
                       "street_address": "456 Oak St",
                       "city": "Othertown",
                       "state": "NY"
                     }
                   }
                   """;

        var errorCollection = new ErrorCollection();
        var result = customerSchema.Validate(JsonDocument.Parse(data), errorCollection);

        Assert.True(result);
    }

    [Fact]
    public void Test_Defs()
    {
        var schema1 = """
                      {
                        "$id": "https://example.com/schemas/address",
                        "type": "object",
                        "properties": {
                          "street_address": { "$anchor": "street_address", "type": "string" },
                          "city": { "type": "string" },
                          "state": { "type": "string" }
                        }, 
                        "required": ["street_address", "city", "state"]
                      }
                      """;

        var schema2 = """
                      {
                        "$id": "https://example.com/schemas/customer",
                      
                        "type": "object",
                        "properties": {
                          "first_name": { "$ref": "#/$defs/name" },
                          "last_name": { "$ref": "#/$defs/name" },
                          "shipping_address": { "$ref": "/schemas/address" },
                          "billing_address": { "$ref": "/schemas/address" }
                        },
                        "required": ["first_name", "last_name", "shipping_address", "billing_address"],
                      
                        "$defs": {
                          "name": { "type": "string" }
                        }
                      }
                      """;

        var repo = SchemaRegistry.Local();

        var addressSchema = JsonSchema.Create(JsonDocument.Parse(schema1), repo);
        var customerSchema = JsonSchema.Create(JsonDocument.Parse(schema2), repo);

        var data = """
                   {
                     "first_name": "John",
                     "last_name": "Doe",
                     "shipping_address": {
                       "street_address": "123 Main St",
                       "city": "Anytown",
                       "state": "CA"
                     },
                     "billing_address": {
                       "street_address": "456 Oak St",
                       "city": "Othertown",
                       "state": "NY"
                     }
                   }
                   """;

        var errorCollection = new ErrorCollection();
        var result = customerSchema.Validate(JsonDocument.Parse(data), errorCollection);

        Assert.True(result);
    }
}