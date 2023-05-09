using Gaia.Domain;

namespace Gaia.AST;

public sealed class BinaryExpression : Expression {
    public BinaryExpression(SyntaxKind token, Expression left, Expression right) {
        OperatorToken = token;
        Left = left;
        Right = right;
        Kind = SyntaxKind.BinaryExpression;
    }

    public Expression Left { get; }
    public Expression Right { get; }
    public SyntaxKind OperatorToken { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
