using Gaia.Domain;

namespace Gaia.AST;

public sealed class InterfaceDeclaration : Statement {
    public InterfaceDeclaration(Identifier name, List<MethodSignature> members) {
        Name = name;
        Members = members;
        Kind = SyntaxKind.InterfaceDeclaration;
    }

    public Identifier Name { get; }
    public List<MethodSignature> Members { get; }
    // public List<HeritageClause> HeritageClauses { get; } = new();
    // public List<HeritageClause> TypeParameters { get; } = new();

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
