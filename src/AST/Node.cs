using Gaia.Domain;

namespace Gaia.AST;

/// <summary>
/// A node can be an expression, statement, or both.
/// It conforms to the visitor pattern.
/// </summary>
public interface Node {
    SyntaxKind Kind { get; }
    TResult Accept<TResult>(Visitor<TResult> visitor);
}
