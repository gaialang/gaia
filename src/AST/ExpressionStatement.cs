using Gaia.Domain;

namespace Gaia.AST;

public class ExpressionStatement : Statement {
    public ExpressionStatement(Expression expr) {
        Expression = expr;
        Kind = SyntaxKind.ExpressionStatement;
    }

    public Expression Expression { get; private set; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
