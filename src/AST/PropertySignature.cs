using Gaia.Domain;

namespace Gaia.AST;

public sealed class PropertySignature : Statement {
    public PropertySignature(Identifier name, Expression typ) {
        Name = name;
        Type = typ;
        Kind = SyntaxKind.PropertySignature;
    }

    public Identifier Name { get; }
    public Expression Type { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
