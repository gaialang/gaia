namespace Gaia.AST;

public class AssignStmt : Stmt {
    public readonly Identifier Id;
    public readonly Expr Expr;

    public AssignStmt(Identifier id, Expr expr) {
        Id = id;
        Expr = expr;
    }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
