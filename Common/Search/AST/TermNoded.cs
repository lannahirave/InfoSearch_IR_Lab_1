using Common.DS.Abstract;

namespace Common.Search.AST;

public class TermNode : QueryNode
{
    public string Term { get; }

    public TermNode(string term)
    {
        // Term should be pre-normalized by the parser
        Term = term ?? throw new ArgumentNullException(nameof(term));
    }

    public override IReadOnlySet<string> Evaluate(IIndexAccessor indexAccessor, EvaluationServices services)
    {
        return indexAccessor.GetDocumentsForTerm(Term);
    }

    public override string ToString() => $"TERM({Term})";
}
