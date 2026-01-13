namespace Bebop.JsonSchema;

internal sealed class SchemaApplicabilityAttribute(SchemaVersion version, string vocabulary) : Attribute
{
    public SchemaVersion Version { get; } = version;

    public string Vocabulary { get; } = vocabulary;
}