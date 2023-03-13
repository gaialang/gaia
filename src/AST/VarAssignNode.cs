namespace Gaia.AST;

public sealed class VarAssignNode : Stmt {
    public VarAssignNode(IdNode i, Node? expr = null) {
        IdNode = i;
        Expr = expr;
        NodeType = NodeType.Var;
    }

    public IdNode IdNode { get; }
    public Node? Expr { get; }
    public override NodeType NodeType { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
