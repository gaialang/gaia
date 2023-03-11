namespace Gaia.AST;

public interface Visitor<TResult, TContext> {
    public TResult Visit(PackageNode expr, TContext ctx);
    public TResult Visit(IdNode expr, TContext ctx);
    public TResult Visit(VarNode expr, TContext ctx);
    public TResult Visit(UnaryNode expr, TContext ctx);
    public TResult Visit(IntLiteralNode expr, TContext ctx);
}
