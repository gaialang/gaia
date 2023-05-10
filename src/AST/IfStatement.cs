using Gaia.Domain;

namespace Gaia.AST;

public class IfStatement : Statement {
    public Expression Expression;
    public Block ThenStatement;
    public Statement? ElseStatement;

    public IfStatement(Expression x, Block thenStatement, Statement? elseStatement = null) {
        Expression = x;
        ThenStatement = thenStatement;
        ElseStatement = elseStatement;
        Kind = SyntaxKind.IfStatement;
    }

    public int Pos { get; }
    public int End { get; }
    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
