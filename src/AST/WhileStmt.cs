namespace Gaia.AST;

public sealed class WhileStmt : Stmt {
    public WhileStmt(string name, List<Identifier> args, IdType returnType) {
        Name = name;
        Arguments = args;
        ReturnType = returnType;
    }

    public string Name { get; }
    public List<Identifier> Arguments { get; }
    public IdType ReturnType { get; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
