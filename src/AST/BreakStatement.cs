namespace Gaia.AST;

public class BreakStatement : Statement {
    public BreakStatement(string l = "") {
        Label = l;
        Kind = SyntaxKind.ReturnStatement;
    }

    public string Label { get; }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
