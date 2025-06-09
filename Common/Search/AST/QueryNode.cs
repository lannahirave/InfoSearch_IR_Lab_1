using Common.DS.Abstract;

namespace Common.Search.AST;

public abstract class QueryNode
{
    /// <summary>
    /// Evaluates this query node to produce a set of matching document IDs.
    /// </summary>
    /// <param name="indexAccessor">Accessor to the underlying index data (e.g., Inverted Index or TDM).</param>
    /// <param name="services">Container for other services needed for evaluation (e.g., suffix tree accessor for wildcards).</param>
    /// <returns>A read-only set of document IDs matching this part of the query.</returns>
    public abstract IReadOnlySet<string> Evaluate(IIndexAccessor indexAccessor, EvaluationServices services);
}