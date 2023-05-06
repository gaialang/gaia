using Gaia.Domain;

namespace Gaia.AST;

public sealed class PackageDeclaration : Statement {
    public Identifier Name { get; }
    /// <summary>
    /// var or func statements.
    /// </summary>
    public readonly List<Statement> Statements;

    public PackageDeclaration(Identifier name, List<Statement> list) {
        Name = name;
        Statements = list;
        Kind = SyntaxKind.PackageDeclaration;
    }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
