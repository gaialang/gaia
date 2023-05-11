using Gaia.Domain;

namespace Gaia.AST;

public sealed class ImportDeclaration : Statement {
    public readonly string ModuleSpecifier;

    public ImportDeclaration(string s, int pos, int end) {
        ModuleSpecifier = s;
        Pos = pos;
        End = end;
        Kind = SyntaxKind.ImportDeclaration;
    }

    public int Pos { get; }
    public int End { get; }
    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
