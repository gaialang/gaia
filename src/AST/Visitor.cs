namespace Gaia.AST;

public interface Visitor<TResult> {
    TResult Visit(PackageDeclaration node);
    TResult Visit(Identifier node);
    TResult Visit(VariableDeclaration node);
    TResult Visit(UnaryExpression node);
    TResult Visit(LiteralLikeNode node);
    TResult Visit(FunctionDeclaration node);
    TResult Visit(WhileStatement node);
    TResult Visit(BinaryExpression node);
    TResult Visit(Block node);
    TResult Visit(AssignStatement node);
    TResult Visit(ReturnStatement node);
    TResult Visit(ImportDeclaration node);
    TResult Visit(IfStatement node);
    TResult Visit(BreakStatement node);
    TResult Visit(DoStatement node);
    TResult Visit(ElementAssignStatement node);
    TResult Visit(ElementAccessExpression node);
    TResult Visit(CallExpression node);
    TResult Visit(ExpressionStatement node);
    TResult Visit(ArrayLiteralExpression node);
    TResult Visit(StructDeclaration node);
    TResult Visit(PropertySignature node);
    TResult Visit(KeywordLikeNode node);
    TResult Visit(Parameter node);
    TResult Visit(ArrayType node);
    TResult Visit(IndexedAccessType node);
    TResult Visit(InterfaceDeclaration node);
    TResult Visit(MethodSignature node);
    TResult Visit(EnumDeclaration node);
    TResult Visit(EnumMember node);
}
