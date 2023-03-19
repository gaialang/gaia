namespace Gaia.AST;

public class Block : Statement {
    public readonly List<Statement> Statements;

    public Block(List<Statement> statements) {
        Statements = statements;
        Kind = SyntaxKind.Block;
    }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
