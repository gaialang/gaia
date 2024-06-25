using Gaia.Domain;

namespace Gaia.AST;

public class Block : Statement {
    public readonly List<Statement> Statements;

    public Block(List<Statement> statements, int pos, int end) {
        Statements = statements;
        Pos = pos;
        End = end;
        Kind = SyntaxKind.Block;
    }

    public int Pos { get; }
    public int End { get; }
    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
