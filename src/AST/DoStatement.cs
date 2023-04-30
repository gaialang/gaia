using Gaia.Domain;

namespace Gaia.AST;

public class DoStatement : Statement {
    public Expression Expression;
    public Block Body;

    public DoStatement(Block body, Expression expr) {
        Body = body;
        Expression = expr;
        Kind = SyntaxKind.DoStatement;
    }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
