namespace Gaia.AST;

public class DoStatement : Statement {
    public Expression Expression;
    public Block Body;

    public DoStatement(Block body, Expression expr) {
        Body = body;
        Expression = expr;
        Kind = SyntaxKind.DoStatement;
    }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
