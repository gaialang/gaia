using Gaia.Domain;

namespace Gaia.AST;

public sealed class PackageDeclaration : Statement {
    public Identifier Name { get; }
    /// <summary>
    /// var or func statements.
    /// </summary>
    public readonly List<Statement> Statements;

    public PackageDeclaration(Identifier name, List<Statement> list, int pos, int end) {
        Name = name;
        Statements = list;
        Pos = pos;
        End = end;
        Kind = SyntaxKind.PackageDeclaration;
    }

    public int Pos { get; }
    public int End { get; }
    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
