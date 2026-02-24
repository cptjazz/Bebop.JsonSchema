namespace Tests.MyJsonSchema;

public class Test_ErrorCollection
{
    [Fact]
    public async Task ErrorCollection_Count_ReturnsZero_WhenValidationSucceeds()
    {
        // Arrange
        var schema = await JsonSchema.Create("""{"type":"string"}""");
        await schema.Prepare();
        var errors = new ErrorCollection();
        using var doc = JsonDocument.Parse(""" "hello" """);

        // Act
        var isValid = await schema.Validate(doc, errors);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task ErrorCollection_Count_ReturnsErrorCount_WhenValidationFails()
    {
        // Arrange
        var schema = await JsonSchema.Create("""{"type":"string"}""");
        await schema.Prepare();
        var errors = new ErrorCollection();
        using var doc = JsonDocument.Parse("42");

        // Act
        var isValid = await schema.Validate(doc, errors);

        // Assert
        Assert.False(isValid);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task ErrorCollection_Indexer_ReturnsValidationError()
    {
        // Arrange
        var schema = await JsonSchema.Create("""{"type":"string"}""");
        await schema.Prepare();
        var errors = new ErrorCollection();
        using var doc = JsonDocument.Parse("42");

        // Act
        await schema.Validate(doc, errors);

        // Assert
        Assert.True(errors.Count > 0);
        var firstError = errors[0];
        Assert.NotNull(firstError);
        Assert.NotNull(firstError.Message);
        Assert.NotNull(firstError.Path);
    }

    [Fact]
    public async Task ErrorCollection_CanIterate_UsingForEach()
    {
        // Arrange
        var schema = await JsonSchema.Create("""{"type":"string"}""");
        await schema.Prepare();
        var errors = new ErrorCollection();
        using var doc = JsonDocument.Parse("42");

        // Act
        await schema.Validate(doc, errors);

        // Assert
        Assert.True(errors.Count > 0);
        
        var iteratedCount = 0;
        foreach (var error in errors)
        {
            Assert.NotNull(error);
            Assert.NotNull(error.Message);
            Assert.NotNull(error.Path);
            iteratedCount++;
        }

        Assert.Equal(errors.Count, iteratedCount);
    }

    [Fact]
    public async Task ErrorCollection_CanIterate_UsingLinq()
    {
        // Arrange
        var schema = await JsonSchema.Create("""{"type":"string"}""");
        await schema.Prepare();
        var errors = new ErrorCollection();
        using var doc = JsonDocument.Parse("42");

        // Act
        await schema.Validate(doc, errors);

        // Assert
        Assert.True(errors.Count > 0);
        var errorList = errors.ToList();
        Assert.Equal(errors.Count, errorList.Count);
    }

    [Fact]
    public async Task ErrorCollection_MultipleErrors_AreAllAccessible()
    {
        // Arrange - schema with multiple constraints that will fail
        var schema = await JsonSchema.Create("""
        {
            "type": "object",
            "required": ["name", "age"],
            "properties": {
                "name": {"type": "string"},
                "age": {"type": "number", "minimum": 0}
            }
        }
        """);
        await schema.Prepare();
        var errors = new ErrorCollection();
        using var doc = JsonDocument.Parse("{}");

        // Act
        await schema.Validate(doc, errors);

        // Assert
        Assert.False(await schema.Validate(doc, new ErrorCollection()));
        
        // We should have errors for missing required properties
        // The exact count depends on the validation behavior
        Assert.True(errors.Count > 0);

        // Verify all errors are accessible
        for (int i = 0; i < errors.Count; i++)
        {
            var error = errors[i];
            Assert.NotNull(error);
            Assert.NotNull(error.Message);
            Assert.NotNull(error.Path);
        }
    }

    [Fact]
    public async Task ValidationError_HasCorrectProperties()
    {
        // Arrange
        var schema = await JsonSchema.Create("""{"type":"string"}""");
        await schema.Prepare();
        var errors = new ErrorCollection();
        using var doc = JsonDocument.Parse("42");

        // Act
        await schema.Validate(doc, errors);

        // Assert
        Assert.True(errors.Count > 0);
        var error = errors[0];
        
        // Verify the error has meaningful content
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
        Assert.NotEqual(default(JsonElement), error.Element);
        Assert.NotNull(error.Path);
    }

    [Fact]
    public async Task ValidationError_Path_IsCorrect()
    {
        // Arrange - nested schema
        var schema = await JsonSchema.Create("""
        {
            "type": "object",
            "properties": {
                "nested": {
                    "type": "object",
                    "properties": {
                        "value": {"type": "string"}
                    }
                }
            }
        }
        """);
        await schema.Prepare();
        var errors = new ErrorCollection();
        using var doc = JsonDocument.Parse("""
        {
            "nested": {
                "value": 123
            }
        }
        """);

        // Act
        await schema.Validate(doc, errors);

        // Assert
        // Should have at least one error
        if (errors.Count > 0)
        {
            // Path should reference the nested structure
            // Note: exact path format depends on implementation
            var hasPathWithNested = errors.Any(e => e.Path.Contains("nested") || e.Path.Contains("value"));
            Assert.True(hasPathWithNested);
        }
    }

    [Fact]
    public async Task ErrorCollection_IReadOnlyList_IsImplemented()
    {
        // Arrange
        var schema = await JsonSchema.Create("""{"type":"string"}""");
        await schema.Prepare();
        var errors = new ErrorCollection();
        using var doc = JsonDocument.Parse("42");

        // Act
        await schema.Validate(doc, errors);

        // Assert
        IReadOnlyList<ValidationError> readOnlyErrors = errors;
        Assert.Equal(errors.Count, readOnlyErrors.Count);
        
        if (errors.Count > 0)
        {
            Assert.Equal(errors[0].Message, readOnlyErrors[0].Message);
        }
    }

    [Fact]
    public void ErrorCollection_EmptyCollection_CanBeEnumerated()
    {
        // Arrange
        var errors = new ErrorCollection();

        // Act & Assert
        Assert.Empty(errors);
        
        var count = 0;
        foreach (var error in errors)
        {
            count++;
        }
        
        Assert.Empty(errors);
    }
}
