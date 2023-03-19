namespace Gaia.AST;

public interface Visitor<TResult, TContext> {
    TResult Visit(PackageStmt node, TContext ctx);
    TResult Visit(Identifier node, TContext ctx);
    TResult Visit(VarStmt node, TContext ctx);
    TResult Visit(UnaryOpExpr node, TContext ctx);
    TResult Visit(IntLiteral node, TContext ctx);
    TResult Visit(FuncStmt node, TContext ctx);
    TResult Visit(WhileStmt node, TContext ctx);
    TResult Visit(BinaryOpExpr node, TContext ctx);
    TResult Visit(StmtList node, TContext ctx);
    TResult Visit(AssignStmt node, TContext ctx);
}
