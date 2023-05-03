using Gaia.Domain;

namespace Gaia.AST;

public class ArrayType : Expression {
    public Expression ElementType { get; }

    public ArrayType(Expression elem, int pos, int end) {
        Kind = SyntaxKind.ArrayType;
        Pos = pos;
        End = end;
        ElementType = elem;
    }

    public int Pos { get; }
    public int End { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
