using Gaia.Compiler;

namespace Gaia.AST;

public sealed class BinaryOpExpr : Expr {
    public BinaryOpExpr(Token token, Expr lhs, Expr rhs) {
        Operator = token;
        Lhs = lhs;
        Rhs = rhs;
    }

    public Expr Lhs { get; }
    public Expr Rhs { get; }
    public Token Operator { get; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
