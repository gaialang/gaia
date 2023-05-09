using Gaia.Domain;

namespace Gaia.AST;

public sealed class Parameter : Expression {
    public Parameter(Identifier name, Expression typ, int pos = -1, int end = -1) {
        Name = name;
        Type = typ;
        Kind = SyntaxKind.Parameter;
        Pos = pos;
        End = end;
    }

    public Identifier Name { get; }
    public Expression Type { get; }

    public int Pos { get; }
    public int End { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
