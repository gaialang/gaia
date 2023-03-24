namespace Gaia.AST;

public class ElementAssignStatement : Statement {
    public ElementAccessExpression Left { get; }
    public Expression Right { get; }

    public ElementAssignStatement(ElementAccessExpression l, Expression r) {
        Left = l;
        Right = r;
        Kind = SyntaxKind.ElementAssignStatement;
    }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
