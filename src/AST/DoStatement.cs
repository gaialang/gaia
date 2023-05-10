using Gaia.Domain;

namespace Gaia.AST;

public class DoStatement : Statement {
    public Expression Expression;
    public Block Body;

    public DoStatement(Block body, Expression expr) {
        Body = body;
        Expression = expr;
        Kind = SyntaxKind.DoStatement;
    }

    public int Pos { get; }
    public int End { get; }
    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
