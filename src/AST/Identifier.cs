namespace Gaia.AST;

public sealed class Identifier : Expr {
    public Identifier(string name, IdType t) {
        Name = name;
        IdType = t;
    }

    public string Name { get; }
    public IdType IdType { get; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
