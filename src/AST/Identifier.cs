using Gaia.Domain;

namespace Gaia.AST;

public sealed class Identifier : Expression {
    public Identifier(string text, int pos, int end) {
        Text = text;
        Pos = pos;
        End = end;
        Kind = SyntaxKind.Identifier;
    }

    public string Text { get; }
    public SyntaxKind Kind { get; }
    public int Pos { get; }
    public int End { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
