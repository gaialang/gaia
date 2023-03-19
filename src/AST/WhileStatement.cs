namespace Gaia.AST;

public sealed class WhileStatement : Statement {
    public WhileStatement(string name) {
        Name = name;
    }

    public string Name { get; }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
