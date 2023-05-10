using Gaia.Domain;

namespace Gaia.AST;

public sealed class BinaryExpression : Expression {
    public BinaryExpression(SyntaxKind token, Expression left, Expression right, int pos, int end) {
        OperatorToken = token;
        Left = left;
        Right = right;
        Pos = pos;
        End = end;
        Kind = SyntaxKind.BinaryExpression;
    }

    public Expression Left { get; }
    public Expression Right { get; }
    public SyntaxKind OperatorToken { get; }

    public int Pos { get; }
    public int End { get; }
    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
