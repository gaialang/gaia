using Gaia.Domain;

namespace Gaia.AST;

public class ElementAssignStatement : Statement {
    public ElementAccessExpression Left { get; }
    public Expression Right { get; }

    public ElementAssignStatement(ElementAccessExpression l, Expression r) {
        Left = l;
        Right = r;
        Kind = SyntaxKind.ElementAssignStatement;
    }

    public int Pos { get; }
    public int End { get; }
    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
