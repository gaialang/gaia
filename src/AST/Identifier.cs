namespace Gaia.AST;

public sealed class Identifier : Expression {
    public Identifier(string name, IdType t) {
        Name = name;
        IdType = t;
        Kind = SyntaxKind.Identifier;
    }

    public string Name { get; }
    public IdType IdType { get; }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
