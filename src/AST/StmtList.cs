namespace Gaia.AST;

public class StmtList : Stmt {
    public readonly Stmt? Head;
    public readonly Stmt? Tail;

    public StmtList(Stmt? head, Stmt? tail) {
        Head = head;
        Tail = tail;
    }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
