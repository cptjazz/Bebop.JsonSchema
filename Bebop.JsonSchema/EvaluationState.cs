using System.Runtime.InteropServices;

namespace Bebop.JsonSchema;

internal readonly struct EvaluationState(EvaluationStateComp comp)
{
    private readonly record struct S(bool ValidationState, JsonSchema Schema);

    private readonly Dictionary<JsonPointer, List<S>> _properties = new();

    public IReadOnlyList<JsonSchema> SchemaStack => comp.SchemaStack;

    internal EvaluationStateComp Comp => comp;

    public EvaluationState New() => new(comp);

    public void Absorb(in EvaluationState other)
    {
        if (other._properties.Count == 0)
            return;

        _AbsorbSlow(this, other);

        static void _AbsorbSlow(in EvaluationState t, in EvaluationState o)
        {
            foreach (var (path, list) in o._properties)
            {
                if (list.Count > 0)
                    t.AddProperties(path, list);
            }
        }
    }

    public void PushSchema(JsonSchema schema)
    {
        comp.PushSchema(schema);
    }

    public void PopSchema()
    {
        comp.PopSchema();
    }

    public void AddProperty(in JsonPointer path, bool evaluationValid)
    {
        ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(_properties, path, out var exists);

        var item = new S(evaluationValid, comp.CurrentSchema!);

        if (!exists)
        {
            list = new List<S>([item]);
        }
        else
        {
            list!.Add(item);
        }
    }

    private void AddProperties(in JsonPointer path, List<S> states)
    {
        ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(_properties, path, out var exists);

        if (!exists)
        {
            list = new List<S>(states);
        }
        else
        {
            list!.AddRange(states);
        }
    }


    public bool HasSeenProperty(in JsonPointer path)
    {
        if (!_properties.TryGetValue(path, out var list)) 
            return false;

        return _HasSeenPropertySlow(list, comp.CurrentSchema!);

        static bool _HasSeenPropertySlow(List<S> li, JsonSchema c)
        {
            for (var i = 0; i < li.Count; i++)
            {
                var l = li[i];
                if (ReferenceEquals(l.Schema, c))
                    return true;
            }

            return false;
        }
    }

    public bool IsPropertyUnevaluated(in JsonPointer path)
    {
        return !_properties.TryGetValue(path, out var list) || _IsUnevaluatedSlow(this, list);

        static bool _IsUnevaluatedSlow(EvaluationState s, List<S> list)
        {
            var ll = new HashSet<JsonSchema>(s.Comp.SchemaTree.Count);
            _RecAdd(ll, s.Comp.CurrentSchema!, s.Comp.SchemaTree);

            static void _RecAdd(
                HashSet<JsonSchema> set, 
                JsonSchema s,
                Dictionary<JsonSchema, List<JsonSchema>> tree)
            {
                if (!set.Add(s))
                    return;

                if (!tree.TryGetValue(s, out var li))
                    return;

                foreach (var l in li)
                    _RecAdd(set, l, tree);
            }

            return list
                .Where(l => ll.Contains(l.Schema))
                .All(l => !l.ValidationState);
        }
    }
}