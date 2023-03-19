namespace Gaia.AST;

public sealed class FuncStmt : Stmt {
    public FuncStmt(string name, List<Identifier> args, IdType returnType, StmtList? body) {
        Name = name;
        Arguments = args;
        ReturnType = returnType;
        Body = body;
    }

    public string Name { get; }
    public List<Identifier> Arguments { get; }
    public IdType ReturnType { get; }
    public StmtList? Body { get; private set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
