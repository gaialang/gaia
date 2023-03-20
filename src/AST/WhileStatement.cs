namespace Gaia.AST;

public sealed class WhileStatement : Statement {
    public WhileStatement(Expression expr, Block body) {
        Expr = expr;
        Body = body;
        Kind = SyntaxKind.WhileStatement;
    }

    public Expression Expr { get; }
    public Block Body { get; }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
