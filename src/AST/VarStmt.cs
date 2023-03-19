namespace Gaia.AST;

public sealed class VarStmt : Stmt {
    public VarStmt(Identifier id, Expr? expr = null) {
        Id = id;
        Expr = expr;
    }

    public Identifier Id { get; }
    public Expr? Expr { get; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
