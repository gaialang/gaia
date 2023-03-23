namespace Gaia.AST;

public class IfStatement : Statement {
    public Expression Expression;
    public Block ThenStatement;
    public Statement? ElseStatement;

    public IfStatement(Expression x, Block thenStatement, Statement? elseStatement = null) {
        Expression = x;
        ThenStatement = thenStatement;
        ElseStatement = elseStatement;
        Kind = SyntaxKind.IfStatement;
    }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
