namespace Gaia.AST;

public class AssignStatement : Statement {
    public readonly Identifier Id;
    public readonly Expression Expr;

    public AssignStatement(Identifier id, Expression expr) {
        Id = id;
        Expr = expr;
        Kind = SyntaxKind.AssignStatement;
    }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
