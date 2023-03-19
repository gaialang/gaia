namespace Gaia.AST;

public sealed class ReturnStatement : Statement {
    public ReturnStatement(Expression? e = null) {
        Expression = e;
        Kind = SyntaxKind.ReturnStatement;
    }

    public Expression? Expression { get; }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
