namespace Gaia.AST;

public sealed class VarAssignNode : Stmt {
    public VarAssignNode(IdNode i, Expr? expr = null) {
        IdNode = i;
        Expr = expr;
        NodeType = NodeType.VarAssign;
    }

    public IdNode IdNode { get; }
    public Expr? Expr { get; }
    public override NodeType NodeType { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
