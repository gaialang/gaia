namespace Gaia.AST;

public interface Node {
    SyntaxKind Kind { get; }
    TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx);
}
