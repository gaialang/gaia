namespace Gaia.AST;

public sealed class ReturnStatement : Statement {
    public ReturnStatement(Expression? e = null) {
        Expression = e;
        Kind = SyntaxKind.ReturnStatement;
    }

    public Expression? Expression { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
