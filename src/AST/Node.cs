namespace Gaia.AST;

public abstract class Node {
    public abstract NodeType NodeType { get; protected set; }

    public abstract TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx);
}
