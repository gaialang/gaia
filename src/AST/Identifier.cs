namespace Gaia.AST;

public sealed class Identifier : Expression {
    public Identifier(string name, TypeInfo t) {
        Name = name;
        TypeInfo = t;
        Kind = SyntaxKind.Identifier;
    }

    public string Name { get; }
    public TypeInfo TypeInfo { get; }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }


}
