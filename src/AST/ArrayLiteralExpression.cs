using Gaia.Domain;

namespace Gaia.AST;

public sealed class ArrayLiteralExpression : Expression {
    public ArrayLiteralExpression(List<Expression> elems, int pos, int end) {
        Elements = elems;
        Pos = pos;
        End = end;
        Kind = SyntaxKind.ArrayLiteralExpression;
    }

    public List<Expression> Elements { get; }
    public int Pos { get; }
    public int End { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
