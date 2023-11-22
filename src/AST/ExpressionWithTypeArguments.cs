using Gaia.Domain;

namespace Gaia.AST;

public sealed class ExpressionWithTypeArguments : Expression {
    public ExpressionWithTypeArguments(Identifier id, int pos, int end) {
        Expression = id;
        Pos = pos;
        End = end;
        Kind = SyntaxKind.ExpressionWithTypeArguments;
    }

    public Identifier Expression { get; }
    public int Pos { get; }
    public int End { get; }
    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
