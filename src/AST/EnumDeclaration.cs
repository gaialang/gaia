using Gaia.Domain;

namespace Gaia.AST;

public sealed class EnumDeclaration : Statement {
    public EnumDeclaration(Identifier name, List<EnumMember> members) {
        Name = name;
        Members = members;
        Kind = SyntaxKind.EnumDeclaration;
    }

    public Identifier Name { get; }
    public List<EnumMember> Members { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
