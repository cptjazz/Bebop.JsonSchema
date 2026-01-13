namespace Bebop.JsonSchema;

internal static class JsonExtensions
{
    public static JsonElement ExpectObject(this JsonProperty property)
    {
        if (property.Value.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidSchemaException("Expected an object.", property);
        }

        return property.Value;
    }

    public static JsonElement ExpectObject(this JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidSchemaException("Expected an object.", element);
        }

        return element;
    }

    public static Uri ExpectUri(this JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.String)
        {
            throw new InvalidSchemaException("Expected a string.", element);
        }

        return new Uri(element.GetString()!, UriKind.RelativeOrAbsolute);
    }

    public static double ExpectNumber(this JsonProperty property)
    {
        if (property.Value.ValueKind != JsonValueKind.Number)
        {
            throw new InvalidSchemaException("Expected a number.", property);
        }

        return property.Value.GetDouble();
    }

    public static int ExpectNonNegativeCount(this JsonProperty property)
    {
        if (property.Value.ValueKind != JsonValueKind.Number)
        {
            throw new InvalidSchemaException("Expected a number.", property);
        }

        int l = -1;
        if (!property.Value.TryGetInt32(out l))
        {
            var d = property.Value.GetDouble();
            if (d < 0 || d % 1 != 0)
            {
                throw new InvalidSchemaException("Expected a non-negative integer.", property);
            }

            l = (int)d;
        }

        return l < 0
            ? throw new InvalidSchemaException("Expected a non-negative number.", property)
            : l;
    }

    public static bool ExpectBoolean(this JsonProperty property)
    {
        if (property.Value.ValueKind != JsonValueKind.True && property.Value.ValueKind != JsonValueKind.False)
        {
            throw new InvalidSchemaException("Expected a boolean.", property);
        }

        return property.Value.GetBoolean();
    }

    public static JsonElement.ArrayEnumerator ExpectArray(this JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidSchemaException("Expected an array.", element);
        }

        return element.EnumerateArray();
    }

    public static JsonElement.ArrayEnumerator ExpectArray(this JsonProperty property)
    {
        if (property.Value.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidSchemaException("Expected an array.", property);
        }

        return property.Value.EnumerateArray();
    }


    public static bool ExpectBool(this JsonElement element)
    {
        if (element.ValueKind is not JsonValueKind.True and not JsonValueKind.False)
        {
            throw new InvalidSchemaException("Expected a number.", element);
        }

        return element.GetBoolean();
    }

    public static long ExpectNumber(this JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Number)
        {
            throw new InvalidSchemaException("Expected a number.", element);
        }

        return element.GetInt64();
    }

    public static bool ExpectBoolean(this JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.True && element.ValueKind != JsonValueKind.False)
        {
            throw new InvalidSchemaException("Expected a boolean.", element);
        }

        return element.GetBoolean();
    }

    public static int ExpectNonNegativeNumber(this JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Number)
        {
            throw new InvalidSchemaException("Expected a number.", element);
        }

        if (element.TryGetInt32(out var val))
        {
            return val < 0
                ? throw new InvalidSchemaException("Expected a non-negative number.", element)
                : val;
        }

        if (element.TryGetDouble(out var d))
        {
            // Find out if d has a fraction part
            if (d < 0 || d % 1 != 0)
            {
                throw new InvalidSchemaException("Expected a non-negative integer.", element);
            }

            return (int)d;
        }

        throw new InvalidSchemaException("Expected a non-negative integer.", element);
    }

    public static string ExpectString(this JsonProperty property)
    {
        if (property.Value.ValueKind != JsonValueKind.String)
        {
            throw new InvalidSchemaException("Expected a string.", property);
        }

        var str = property.Value.GetString();
        return str ?? throw new InvalidSchemaException("Expected a non-null string.", property);
    }

    public static string ExpectString(this JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.String)
        {
            throw new InvalidSchemaException("Expected a string.", element);
        }

        var str = element.GetString();
        return str ?? throw new InvalidSchemaException("Expected a non-null string.", element);
    }

#if NET8_0

    public static int GetPropertyCount(this in JsonElement element)
    {
        return element.EnumerateObject().Count();
    }

#endif
}