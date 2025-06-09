namespace Common.Search.AST;

/// <summary>
/// Container for additional services that might be needed during query evaluation,
/// especially for advanced features like wildcards.
/// </summary>
public class EvaluationServices
{
    // Example for future:
    // public ISuffixTreeAccessor SuffixTree { get; }
    // public IKgramIndexAccessor KgramIndex { get; }

    // For now, it can be empty or hold things like the token normalizer if needed at evaluation.
    // However, terms should ideally be normalized during parsing.

    // public EvaluationServices(/* ISuffixTreeAccessor suffixTree, ... */)
    // {
    //    SuffixTree = suffixTree;
    // }
}
