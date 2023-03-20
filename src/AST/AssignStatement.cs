namespace Gaia.AST;

public class AssignStatement : Statement {
    public readonly Identifier LValue;
    public readonly Expression RValue;

    public AssignStatement(Identifier lvalue, Expression rvalue) {
        LValue = lvalue;
        RValue = rvalue;
        Kind = SyntaxKind.AssignStatement;
    }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
