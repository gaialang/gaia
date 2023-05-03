namespace Gaia.AST;

public interface Visitor<TResult, TContext> {
    TResult Visit(PackageDeclaration node, TContext ctx);
    TResult Visit(Identifier node, TContext ctx);
    TResult Visit(VariableDeclaration node, TContext ctx);
    TResult Visit(UnaryExpression node, TContext ctx);
    TResult Visit(LiteralLikeNode node, TContext ctx);
    TResult Visit(FunctionDeclaration node, TContext ctx);
    TResult Visit(WhileStatement node, TContext ctx);
    TResult Visit(BinaryExpression node, TContext ctx);
    TResult Visit(Block node, TContext ctx);
    TResult Visit(AssignStatement node, TContext ctx);
    TResult Visit(ReturnStatement node, TContext ctx);
    TResult Visit(ImportDeclaration node, TContext ctx);
    TResult Visit(IfStatement node, TContext ctx);
    TResult Visit(BreakStatement node, TContext ctx);
    TResult Visit(DoStatement node, TContext ctx);
    TResult Visit(ElementAssignStatement node, TContext ctx);
    TResult Visit(ElementAccessExpression node, TContext ctx);
    TResult Visit(CallExpression node, TContext ctx);
    TResult Visit(ExpressionStatement node, TContext ctx);
    TResult Visit(ArrayLiteralExpression node, TContext ctx);
    TResult Visit(StructDeclaration node, TContext ctx);
    TResult Visit(PropertySignature node, TContext ctx);
    TResult Visit(KeywordLikeNode node, TContext ctx);
    TResult Visit(Parameter node, TContext ctx);
    TResult Visit(ArrayType node, TContext ctx);
    TResult Visit(IndexedAccessType node, TContext ctx);
}
