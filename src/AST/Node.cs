namespace Gaia.AST;

public abstract class Node {
    public abstract TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx);
}
