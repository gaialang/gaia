using Gaia.Domain;

namespace Gaia.AST;

public class AssignStatement : Statement {
    public Identifier Left { get; }
    public Expression Right { get; }

    public AssignStatement(Identifier l, Expression r) {
        Left = l;
        Right = r;
        Kind = SyntaxKind.AssignStatement;
    }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
