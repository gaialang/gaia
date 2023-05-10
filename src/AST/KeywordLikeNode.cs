using Gaia.Domain;

namespace Gaia.AST;

public class KeywordLikeNode : Expression {
    public KeywordLikeNode(SyntaxKind kind, int pos, int end) {
        Kind = kind;
        Pos = pos;
        End = end;
    }

    public int Pos { get; }
    public int End { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
