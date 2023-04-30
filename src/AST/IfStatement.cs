using Gaia.Domain;

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

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
