using Gaia.Compiler;

namespace Gaia.AST;

public sealed class UnaryExpression : Expression {
    public Token Operator { get; }
    public Node Operand { get; }
    public UnaryExpression(Token op, Node operand) {
        Operator = op;
        Operand = operand;
        Kind = SyntaxKind.UnaryExpression;
    }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}