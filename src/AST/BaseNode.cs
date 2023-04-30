using Gaia.Domain;

namespace Gaia.AST;

public class BaseNode : Expression {
    public BaseNode(SyntaxKind kind, int pos, int end) {
        Kind = kind;
        Pos = pos;
        End = end;
    }

    public int Pos { get; }
    public int End { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
