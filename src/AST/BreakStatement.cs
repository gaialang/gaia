using Gaia.Domain;

namespace Gaia.AST;

public class BreakStatement : Statement {
    public BreakStatement(string l = "") {
        Label = l;
        Kind = SyntaxKind.ReturnStatement;
    }

    public string Label { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
