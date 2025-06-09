namespace Common.Search.AST;

public class QueryToken
{
    public QueryTokenType Type { get; }
    public string? Value { get; } // For Term, WildcardTerm; null for operators/parens

    public QueryToken(QueryTokenType type, string? value = null)
    {
        Type = type;
        Value = value;
    }

    public override string ToString()
    {
        return Value != null ? $"{Type}({Value})" : Type.ToString();
    }
}
