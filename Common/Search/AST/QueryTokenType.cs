namespace Common.Search.AST;

public enum QueryTokenType
{
    Term,         // A search term
    And,          // AND operator
    Or,           // OR operator
    Not,          // NOT operator
    LParenthesis,       // Left parenthesis (
    RParenthesis,       // Right parenthesis )
    WildcardTerm, // For future use: e.g., comp*er
    EndOfQuery    // Special token to mark the end
}
