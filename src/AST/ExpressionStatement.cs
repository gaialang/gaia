using Gaia.Domain;

namespace Gaia.AST;

public class ExpressionStatement : Statement {
    public ExpressionStatement(Expression expr) {
        Expression = expr;
        Kind = SyntaxKind.ExpressionStatement;
    }

    public Expression Expression { get; private set; }
    public int Pos { get; }
    public int End { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
