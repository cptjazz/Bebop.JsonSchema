using System.Runtime.CompilerServices;

namespace Bebop.JsonSchema.Assertions.Applicator;

[SchemaApplicability(SchemaVersion.Draft2020_12, Vocabularies_Draft202012.Applicator)]
internal sealed class NotAssertion(JsonSchema schema) : Assertion
{
    public override async ValueTask<bool> Assert(Token element, EvaluationState evaluationState, ErrorCollection errorCollection)
    {
        var es = evaluationState.New();
        var result = await schema.Validate(element, es, NopErrorCollection.Instance).ConfigureAwait(false);

        if (!result)
        {
            evaluationState.Absorb(es);
            return true;
        }

        return _AddError(errorCollection, element);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool _AddError(ErrorCollection ec, in Token e)
        {
            ec.AddError("Element matches schema in 'not' assertion.", e);
            return false;
        }
    }

    public override ValueTask PrepareImpl()
    {
        return schema.Prepare();
    }
}