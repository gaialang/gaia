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

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}