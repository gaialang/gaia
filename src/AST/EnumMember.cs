using Gaia.Domain;

namespace Gaia.AST;

public sealed class EnumMember : Statement {
    public EnumMember(Identifier name, Expression? expr = null) {
        Name = name;
        Initializer = expr;
        Kind = SyntaxKind.EnumMember;
    }

    public Identifier Name { get; }
    public Expression? Initializer { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
