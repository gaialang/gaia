using Gaia.Domain;

namespace Gaia.AST;

public sealed class HeritageClause : Expression {
    public HeritageClause(List<ExpressionWithTypeArguments> types, int pos, int end) {
        Types = types;
        Pos = pos;
        End = end;
        Kind = SyntaxKind.HeritageClause;
    }

    public List<ExpressionWithTypeArguments> Types { get; }
    public int Pos { get; }
    public int End { get; }
    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
