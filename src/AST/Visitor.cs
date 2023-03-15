namespace Gaia.AST;

public interface Visitor<TResult, TContext> {
    public TResult Visit(PackageNode expr, TContext ctx);
    public TResult Visit(IdNode expr, TContext ctx);
    public TResult Visit(VarAssignNode expr, TContext ctx);
    public TResult Visit(UnaryOpNode expr, TContext ctx);
    public TResult Visit(IntLiteralNode expr, TContext ctx);
    public TResult Visit(FuncNode expr, TContext ctx);
    public TResult Visit(WhileNode expr, TContext ctx);
    public TResult Visit(BinaryOpNode expr, TContext ctx);
}
