using Gaia.Domain;

namespace Gaia.AST;

public sealed class UnaryExpression : Expression {
    public SyntaxKind Operator { get; }
    public Expression Operand { get; }
    public UnaryExpression(SyntaxKind op, Expression operand) {
        Operator = op;
        Operand = operand;
        Kind = SyntaxKind.UnaryExpression;
    }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
