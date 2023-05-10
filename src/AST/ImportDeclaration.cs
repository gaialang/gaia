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

    public int Pos { get; }
    public int End { get; }
    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
