using Gaia.Domain;

namespace Gaia.AST;

public sealed class ImportDeclaration : Statement {
    public readonly string ModuleSpecifier;
    /// <summary>
    /// var or func statements.
    /// </summary>

    public ImportDeclaration(string s) {
        ModuleSpecifier = s;
        Kind = SyntaxKind.ImportDeclaration;
    }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
