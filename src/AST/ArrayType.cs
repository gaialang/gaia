using Gaia.Domain;

namespace Gaia.AST;

public class ArrayType : BaseNode {
    public Expression ElementType { get; }

    public ArrayType(Expression elem, int pos, int end) : base(SyntaxKind.ArrayType, pos, end) {
        ElementType = elem;
    }
}
