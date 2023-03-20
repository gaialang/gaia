namespace Gaia.AST;

public class ElseStatement : Statement {
    public Block Body;

    public ElseStatement(Block b) {
        Body = b;
        Kind = SyntaxKind.ElseStatement;
    }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
