namespace Gaia.AST;

public sealed class FloatLiteral : Expression {
    public FloatLiteral(string text, int pos) {
        Text = text;
        Pos = pos;
        End = pos + text.Length;
        Kind = SyntaxKind.IntLiteral;
    }

    public string Text { get; }
    public int Pos { get; }
    public int End { get; }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
