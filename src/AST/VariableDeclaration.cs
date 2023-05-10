using Gaia.Domain;

namespace Gaia.AST;

public sealed class VariableDeclaration : Statement {
    public VariableDeclaration(Identifier id, Expression typ, Expression? expr, int pos, int end) {
        Name = id;
        Type = typ;
        Initializer = expr;
        Pos = pos;
        End = end;
        Kind = SyntaxKind.VariableDeclaration;
    }

    public Identifier Name { get; }
    public Expression? Initializer { get; }
    public Expression Type { get; }
    public int Pos { get; }
    public int End { get; }
    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
