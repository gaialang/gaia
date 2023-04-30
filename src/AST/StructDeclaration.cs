using Gaia.Domain;

namespace Gaia.AST;

public sealed class StructDeclaration : Statement {
    public StructDeclaration(Identifier name, List<PropertySignature> members) {
        Name = name;
        Members = members;
        Kind = SyntaxKind.StructDeclaration;
    }

    public Identifier Name { get; }
    public List<PropertySignature> Members { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
