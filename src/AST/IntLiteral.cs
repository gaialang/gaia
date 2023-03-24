namespace Gaia.AST;

public sealed class IntLiteral : Expression {
    public IntLiteral(string text, int pos) {
        Text = text;
        Pos = pos;
        End = pos + text.Length;
        Kind = SyntaxKind.IntLiteral;
    }

    public string Text { get; }
    public int Pos { get; }
    public int End { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
