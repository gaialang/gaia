namespace Gaia.AST;

public class IfStatement : Statement {
    public Expression Expression;
    public Block Body;
    public Statement? ElseStatement;

    public IfStatement(Expression x, Block body, Statement? elseStatement = null) {
        Expression = x;
        Body = body;
        ElseStatement = elseStatement;
        Kind = SyntaxKind.IfStatement;
    }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
