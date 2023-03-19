namespace Gaia.AST;

public sealed class PackageStmt : Stmt {
    public readonly string Name;
    public readonly List<Stmt> VarOrFuncStatements;

    public PackageStmt(string name, List<Stmt> list) {
        Name = name;
        VarOrFuncStatements = list;
    }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
