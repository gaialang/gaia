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

    // public TResult Accept<TResult>(Visitor<TResult> visitor) {
    //     return visitor.Visit(this);
    // }
}
