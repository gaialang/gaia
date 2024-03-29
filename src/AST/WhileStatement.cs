using Gaia.Domain;

namespace Gaia.AST;

public sealed class WhileStatement : Statement {
    public WhileStatement(Expression expr, Block body) {
        Expression = expr;
        Body = body;
        Kind = SyntaxKind.WhileStatement;
    }

    public Expression Expression { get; }
    public Block Body { get; }
    public int Pos { get; }
    public int End { get; }
    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
