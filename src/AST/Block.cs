using Gaia.Domain;

namespace Gaia.AST;

public class Block : Statement {
    public readonly List<Statement> Statements;

    public Block(List<Statement> statements) {
        Statements = statements;
        Kind = SyntaxKind.Block;
    }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
