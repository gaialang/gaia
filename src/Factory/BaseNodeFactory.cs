using Gaia.Domain;
using Gaia.AST;

namespace Gaia.Factory;

public static class BaseNodeFactory {
    public static Node CreateBaseTokenNode(SyntaxKind kind) {
        return new KeywordLikeNode(kind, -1, -1);
    }

    public static Expression CreateBaseLiteralNode(SyntaxKind kind, string text) {
        return new LiteralLikeNode(kind, text, -1, -1);
    }
}
