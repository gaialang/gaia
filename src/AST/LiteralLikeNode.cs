using Gaia.Domain;

namespace Gaia.AST;

public class LiteralLikeNode : BaseNode {
    public LiteralLikeNode(SyntaxKind kind, int pos, int end, string text) : base(kind, pos, end) {
        Text = text;
    }

    public string Text { get; }
}
