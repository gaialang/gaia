using Gaia.Compiler;

namespace Gaia.AST;

public sealed class UnaryOpExpr : Expr {
    public Token Operator { get; }
    public Node Operand { get; }
    public UnaryOpExpr(Token op, Node operand) {
        Operator = op;
        Operand = operand;
    }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
