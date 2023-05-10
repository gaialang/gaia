using Gaia.Domain;

namespace Gaia.AST;

public class CallExpression : Expression {
    public CallExpression(Expression expr, List<Expression> args) {
        Expression = expr;
        Arguments = args;
        Kind = SyntaxKind.CallExpression;
    }

    public Expression Expression { get; private set; }
    public List<Expression> Arguments { get; private set; }

    public int Pos { get; }
    public int End { get; }
    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
