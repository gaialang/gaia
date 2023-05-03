using Gaia.Domain;

namespace Gaia.AST;

public sealed class Parameter : Expression {
    public Parameter(Identifier name, Expression typ, int pos, int end) {
        Name = name;
        Type = typ;
        Kind = SyntaxKind.PropertySignature;
        Pos = pos;
        End = end;
    }

    public Identifier Name { get; }
    public Expression Type { get; }

    public int Pos { get; }
    public int End { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
