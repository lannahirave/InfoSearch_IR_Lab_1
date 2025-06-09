using System.Text.RegularExpressions;
using Common.Search.AST;
using Common.TextProcessing.Abstract;

namespace Common.Search;

public class QueryParser
{
    private readonly ITokenNormalizer _normalizer;
    private List<QueryToken> _tokens = new();
    private int _currentTokenIndex;

    public QueryParser(ITokenNormalizer normalizer)
    {
        _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
    }

    // --- Tokenizer ---
    private List<QueryToken> Tokenize(string rawQuery)
    {
        var tokens = new List<QueryToken>();
        if (string.IsNullOrWhiteSpace(rawQuery))
        {
            tokens.Add(new QueryToken(QueryTokenType.EndOfQuery));
            return tokens;
        }

        // Regex to capture operators, parentheses, or terms (sequences of non-whitespace, non-operator, non-parenthesis characters)
        // This is a simple regex; more robust tokenization might handle quoted phrases differently.
        var regex = new Regex(@"(\bAND\b|\bOR\b|\bNOT\b|\(|\)|[^\s\(\)]+)", RegexOptions.IgnoreCase);
        var matches = regex.Matches(rawQuery);

        foreach (Match match in matches)
        {
            var value = match.Value;
            if (string.Equals(value, "AND", StringComparison.OrdinalIgnoreCase))
                tokens.Add(new QueryToken(QueryTokenType.And));
            else if (string.Equals(value, "OR", StringComparison.OrdinalIgnoreCase))
                tokens.Add(new QueryToken(QueryTokenType.Or));
            else if (string.Equals(value, "NOT", StringComparison.OrdinalIgnoreCase))
                tokens.Add(new QueryToken(QueryTokenType.Not));
            else if (value == "(")
                tokens.Add(new QueryToken(QueryTokenType.LParenthesis));
            else if (value == ")")
                tokens.Add(new QueryToken(QueryTokenType.RParenthesis));
            else // It's a term
            {
                // For now, assume no wildcards. Wildcard detection would go here.
                // string normalizedTerm = _normalizer.Normalize(value); // Normalization happens here
                // if (!string.IsNullOrWhiteSpace(normalizedTerm))
                // {
                //     tokens.Add(new QueryToken(QueryTokenType.Term, normalizedTerm));
                // }
                // else
                // {
                //     // Handle case where a "term" normalizes to empty (e.g., if it was just punctuation)
                //     // Potentially throw an error or ignore, depending on desired behavior.
                //     // For now, let's assume valid terms. If a term like "---" is passed, it might become empty.
                // }
                // Defer normalization to when TermNode is created to allow parser to see original for wildcard check
                tokens.Add(new QueryToken(QueryTokenType.Term, value));
            }
        }

        tokens.Add(new QueryToken(QueryTokenType.EndOfQuery));
        return tokens;
    }

    // --- Parser Helper Methods ---
    private QueryToken CurrentToken => _tokens[_currentTokenIndex];

    private void ConsumeToken()
    {
        if (_currentTokenIndex < _tokens.Count - 1) // Don't go past EndOfQuery
            _currentTokenIndex++;
    }

    private QueryToken Expect(QueryTokenType expectedType)
    {
        var token = CurrentToken;
        if (token.Type == expectedType)
        {
            ConsumeToken();
            return token;
        }

        throw new QueryParseException(
            $"Syntax Error: Expected {expectedType} but found {token.Type}{(token.Value != null ? $" ('{token.Value}')" : "")} at position near '{string.Join(" ", _tokens.Skip(_currentTokenIndex).Take(3).Select(t => t.Value ?? t.Type.ToString()))}'.");
    }

    // --- Parsing Methods (Recursive Descent) ---
    // Expression ::= OrExpression
    public QueryNode Parse(string rawQuery)
    {
        _tokens = Tokenize(rawQuery);
        _currentTokenIndex = 0;

        if (!_tokens.Any() || _tokens[0].Type == QueryTokenType.EndOfQuery)
        {
            // Handle empty query: return a node that evaluates to an empty set, or throw.
            // For now, let's allow it and it might evaluate to empty based on how TermNode handles empty terms.
            // Or, more robustly, throw an error or return a specific "EmptyQueryNode".
            // For simplicity, we'll let it flow, but an empty query should probably be an error or handled.
            // A single term query like "word" will be parsed by OrExpression -> AndExpression -> NotExpression -> Factor -> Term
            throw new QueryParseException("Query cannot be empty.");
        }

        var queryAst = ParseOrExpression();

        // After parsing the main expression, we should be at the EndOfQuery token
        if (CurrentToken.Type != QueryTokenType.EndOfQuery)
        {
            throw new QueryParseException(
                $"Syntax Error: Unexpected token '{CurrentToken.Value ?? CurrentToken.Type.ToString()}' after complete expression.");
        }

        return queryAst;
    }

    // OrExpression ::= AndExpression (OR AndExpression)*
    private QueryNode ParseOrExpression()
    {
        var node = ParseAndExpression();
        while (CurrentToken.Type == QueryTokenType.Or)
        {
            ConsumeToken(); // Consume 'OR'
            var rightNode = ParseAndExpression();
            node = new OrNode(node, rightNode);
        }

        return node;
    }

    // AndExpression ::= NotExpression (AND NotExpression)*
    private QueryNode ParseAndExpression()
    {
        var node = ParseNotExpression(); // Or ParseUnaryNot
        while (CurrentToken.Type == QueryTokenType.And)
        {
            ConsumeToken(); // Consume 'AND'
            var rightNode = ParseNotExpression(); // Or ParseUnaryNot
            node = new AndNode(node, rightNode);
        }

        return node;
    }

    // NotExpression ::= (NOT)* Factor
    // This handles sequences like NOT NOT term
    private QueryNode ParseNotExpression()
    {
        if (CurrentToken.Type == QueryTokenType.Not)
        {
            ConsumeToken(); // Consume 'NOT'
            // Recursively call ParseNotExpression to handle multiple NOTs or NOT followed by a parenthesized expression
            var operand = ParseNotExpression();
            return new NotNode(operand);
        }

        return ParseFactor();
    }

    // Factor ::= Term | LParen OrExpression RParen
    private QueryNode ParseFactor()
    {
        if (CurrentToken.Type == QueryTokenType.Term)
        {
            var termToken = CurrentToken;
            ConsumeToken(); // Consume Term

            // Normalize the term value here before creating the TermNode
            string normalizedTerm = _normalizer.Normalize(termToken.Value!);
            if (string.IsNullOrWhiteSpace(normalizedTerm))
            {
                // This can happen if the original term was e.g., only punctuation.
                // Decide how to handle: throw error, or treat as a term that matches nothing.
                // For now, let's throw an error as an empty normalized term is usually not intended for search.
                throw new QueryParseException($"Invalid term: '{termToken.Value}' normalizes to an empty string.");
            }

            // Here you would check for wildcards in termToken.Value (before normalization)
            // if (termToken.Value!.Contains("*") || termToken.Value!.Contains("?"))
            // {
            //     return new WildcardTermNode(termToken.Value!); // Pass original value for wildcard processing
            // }
            return new TermNode(normalizedTerm);
        }

        if (CurrentToken.Type == QueryTokenType.LParenthesis)
        {
            ConsumeToken(); // Consume '('
            var node = ParseOrExpression(); // Start parsing from the highest precedence within parentheses
            Expect(QueryTokenType.RParenthesis); // Consume ')'
            return node;
        }

        throw new QueryParseException($"Syntax Error: Unexpected token {CurrentToken.Type}. Expected Term or LParen.");
    }
}

public class QueryParseException : Exception
{
    public QueryParseException(string message) : base(message)
    {
    }
}
