namespace Gaia.AST;

public interface Visitor<TResult, TContext> {
    TResult Visit(PackageDeclaration node, TContext ctx);
    TResult Visit(Identifier node, TContext ctx);
    TResult Visit(VariableDeclaration node, TContext ctx);
    TResult Visit(UnaryExpression node, TContext ctx);
    TResult Visit(IntLiteral node, TContext ctx);
    TResult Visit(FunctionDeclaration node, TContext ctx);
    TResult Visit(WhileStatement node, TContext ctx);
    TResult Visit(BinaryExpression node, TContext ctx);
    TResult Visit(Block node, TContext ctx);
    TResult Visit(AssignStatement node, TContext ctx);
    TResult Visit(ReturnStatement node, TContext ctx);
    TResult Visit(ImportDeclaration node, TContext ctx);
}
