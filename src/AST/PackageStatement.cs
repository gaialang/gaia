namespace Gaia.AST;

public sealed class PackageStatement : Statement {
    public readonly string Name;
    /// <summary>
    /// var or func statements.
    /// </summary>
    public readonly List<Statement> Statements;

    public PackageStatement(string name, List<Statement> list) {
        Name = name;
        Statements = list;
        Kind = SyntaxKind.PackageDeclaration;
    }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
