using Common.DS.Abstract;

namespace Common.Search.AST;

public class OrNode : QueryNode // Binary operation
{
    public QueryNode Left { get; }
    public QueryNode Right { get; }

    public OrNode(QueryNode left, QueryNode right)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }

    public override IReadOnlySet<string> Evaluate(IIndexAccessor indexAccessor, EvaluationServices services)
    {
        var leftDocs = Left.Evaluate(indexAccessor, services);
        var rightDocs = Right.Evaluate(indexAccessor, services);

        // Start with leftDocs and add docs from rightDocs.
        // Create a new HashSet from leftDocs to avoid modifying a potentially shared set.
        var result = new HashSet<string>(leftDocs, StringComparer.OrdinalIgnoreCase);
        result.UnionWith(rightDocs);
        return result;
    }

    public override string ToString() => $"({Left} OR {Right})";
}
