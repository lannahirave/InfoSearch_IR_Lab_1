using Common.DS.Abstract;

namespace Common.Search.AST;

public class AndNode : QueryNode // Binary operation
{
    public QueryNode Left { get; }
    public QueryNode Right { get; }

    public AndNode(QueryNode left, QueryNode right)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }

    public override IReadOnlySet<string> Evaluate(IIndexAccessor indexAccessor, EvaluationServices services)
    {
        var leftDocs = Left.Evaluate(indexAccessor, services);

        // Short-circuit: if the left side yields no documents, the AND result is empty.
        if (!leftDocs.Any())
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var rightDocs = Right.Evaluate(indexAccessor, services);
            
        // Start with leftDocs and find intersection.
        // Create a new HashSet from leftDocs to avoid modifying a potentially shared set.
        var result = new HashSet<string>(leftDocs, StringComparer.OrdinalIgnoreCase);
        result.IntersectWith(rightDocs);
        return result;
    }

    public override string ToString() => $"({Left} AND {Right})";
}