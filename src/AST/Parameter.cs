using Gaia.Domain;

namespace Gaia.AST;

public sealed class Parameter : BaseNode {
    public Parameter(Identifier name, Expression typ, int pos, int end) : base(SyntaxKind.PropertySignature, pos, end) {
        Name = name;
        Type = typ;
    }

    public Identifier Name { get; }
    public Expression Type { get; }
}
