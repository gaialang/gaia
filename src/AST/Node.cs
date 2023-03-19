namespace Gaia.AST;

public abstract class Node {
    public abstract SyntaxKind Kind { get; protected set; }
    public abstract TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx);
}
