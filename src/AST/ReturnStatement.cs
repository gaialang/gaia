using Gaia.Domain;

namespace Gaia.AST;

public sealed class ReturnStatement : Statement {
    public ReturnStatement(Expression? e = null) {
        Expression = e;
        Kind = SyntaxKind.ReturnStatement;
    }

    public Expression? Expression { get; }
    public int Pos { get; }
    public int End { get; }
    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
