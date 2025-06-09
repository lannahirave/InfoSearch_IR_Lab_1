using Common.DS.Abstract;

namespace Common.Search.AST;

public class NotNode : QueryNode // NotNode directly inherits QueryNode, it's a unary operator on a result set.
{
    public QueryNode Operand { get; }

    public NotNode(QueryNode operand)
    {
        Operand = operand ?? throw new ArgumentNullException(nameof(operand));
    }

    public override IReadOnlySet<string> Evaluate(IIndexAccessor indexAccessor, EvaluationServices services)
    {
        var operandDocs = Operand.Evaluate(indexAccessor, services);
        var allDocs = indexAccessor.GetAllDocumentIds();

        // Start with all documents and remove those that match the operand.
        // Ensure a new HashSet is created so 'allDocs' is not modified if it's a shared instance.
        var result = new HashSet<string>(allDocs, StringComparer.OrdinalIgnoreCase);
        result.ExceptWith(operandDocs);
        return result;
    }

    public override string ToString() => $"NOT({Operand})";
}