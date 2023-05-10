using Gaia.Domain;

namespace Gaia.AST;

public class AssignStatement : Statement {
    public Identifier Left { get; }
    public Expression Right { get; }

    public AssignStatement(Identifier l, Expression r, int pos, int end) {
        Left = l;
        Right = r;
        Pos = pos;
        End = end;
        Kind = SyntaxKind.AssignStatement;
    }

    public int Pos { get; }
    public int End { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
