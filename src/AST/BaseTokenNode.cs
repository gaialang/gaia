using Gaia.Domain;

namespace Gaia.AST;

public class BaseTokenNode : BaseNode {
    public BaseTokenNode(SyntaxKind kind, int pos, int end) : base(kind, pos, end) {
    }
}
