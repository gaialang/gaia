namespace Gaia.AST;

public class BreakStatement : Statement {
    public BreakStatement(string l = "") {
        Label = l;
        Kind = SyntaxKind.ReturnStatement;
    }

    public string Label { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
