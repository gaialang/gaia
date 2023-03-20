namespace Gaia.AST;

public sealed class VariableDeclaration : Statement {
    public VariableDeclaration(Identifier id, Expression? expr = null) {
        Id = id;
        Expr = expr;
        Kind = SyntaxKind.VariableDeclaration;
    }

    public Identifier Id { get; }
    public Expression? Expr { get; }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
