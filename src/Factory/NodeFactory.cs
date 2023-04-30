using Gaia.Domain;
using Gaia.AST;

namespace Gaia.Factory;

public static class NodeFactory {
    public static Node CreateToken(SyntaxKind kind) {
        return BaseNodeFactory.CreateBaseTokenNode(kind);
    }

    public static Expression createLiteralLikeNode(SyntaxKind kind,string text) {
        return BaseNodeFactory.CreateBaseLiteralNode(kind, text);
    }
}
