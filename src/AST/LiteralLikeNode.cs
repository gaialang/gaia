using Gaia.Domain;

namespace Gaia.AST;

public class LiteralLikeNode : Expression {
    public LiteralLikeNode(SyntaxKind kind, string text, int pos = -1, int end = -1) {
        Text = text;
        Kind = kind;
        Pos = pos;
        End = end;
    }

    public string Text { get; }

    public int Pos { get; }
    public int End { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
