using Gaia.Domain;

namespace Gaia.AST;

// TODO:
public sealed class SourceFile {
    public readonly string fileName;
    /// <summary>
    /// var or func statements.
    /// </summary>
    public readonly List<Statement> Statements;

    public SourceFile(string name, List<Statement> list) {
        fileName = name;
        Statements = list;
        Kind = SyntaxKind.SourceFile;
    }

    public SyntaxKind Kind { get; }

    // public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
    //     return v.Visit(this, ctx);
    // }
}
