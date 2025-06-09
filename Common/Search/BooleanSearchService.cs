using Common.DS.Abstract;
using Common.Search.AST;

namespace Common.Search;

public class BooleanSearchService
{
    private readonly EvaluationServices _evaluationServices;

    public BooleanSearchService()
    {
        // Initialize any services needed for evaluation.
        // For now, EvaluationServices is simple, but it could hold references
        // to suffix trees, k-gram indexes, etc., in the future.
        _evaluationServices = new EvaluationServices();
    }

    /// <summary>
    /// Executes a parsed query (represented by an AST node) against the given index.
    /// </summary>
    /// <param name="queryAstRoot">The root node of the Abstract Syntax Tree for the query.</param>
    /// <param name="indexAccessor">The accessor for the index data (Inverted Index or Term-Document Matrix).</param>
    /// <returns>A read-only set of document IDs matching the query.</returns>
    public IReadOnlySet<string> ExecuteQuery(QueryNode queryAstRoot, IIndexAccessor indexAccessor)
    {
        if (queryAstRoot == null) throw new ArgumentNullException(nameof(queryAstRoot));
        if (indexAccessor == null) throw new ArgumentNullException(nameof(indexAccessor));

        // The core of the execution is simply calling Evaluate on the AST root.
        // The AST nodes themselves contain the logic for how they are evaluated.
        return queryAstRoot.Evaluate(indexAccessor, _evaluationServices);
    }
}
