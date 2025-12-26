using System.Runtime.InteropServices;

namespace Bebop.JsonSchema;

internal sealed class EvaluationStateComp(int allocSize)
{
    public readonly List<JsonSchema> SchemaStack = new(allocSize);

    public readonly Dictionary<JsonSchema, List<JsonSchema>> SchemaTree = new(allocSize);
    public JsonSchema? CurrentSchema;

    public void PushSchema(JsonSchema schema)
    {
        ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(SchemaTree, schema, out var exists);

        if (!exists)
        {
            list = new List<JsonSchema>((int)Math.Max(0, Math.Log(allocSize)));
        }

        if (CurrentSchema is not null)
        {
            SchemaTree[CurrentSchema!].Add(schema);
        }

        SchemaStack.Add(schema);
        CurrentSchema = schema;
    }

    public void PopSchema()
    {
        SchemaStack.RemoveAt(SchemaStack.Count - 1);
        CurrentSchema = SchemaStack.Count > 0 ? SchemaStack[^1] : null;
    }
}