using Gaia.Domain;

namespace Gaia.AST;

public sealed class UnaryExpression : Expression {
    public SyntaxKind Operator { get; }
    public Node Operand { get; }
    public UnaryExpression(SyntaxKind op, Node operand) {
        Operator = op;
        Operand = operand;
        Kind = SyntaxKind.UnaryExpression;
    }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
