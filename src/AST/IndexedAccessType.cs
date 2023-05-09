using Gaia.Domain;

namespace Gaia.AST;

public class IndexedAccessType : Expression {
    public Expression ObjectType { get; }
    /// <summary>
    /// Empty string means size not specified.
    /// </summary>
    public string IndexType { get; }

    public IndexedAccessType(Expression obj, string index, int pos, int end) {
        ObjectType = obj;
        IndexType = index;
        Kind = SyntaxKind.IndexedAccessType;
        Pos = pos;
        End = end;
    }

    public int Pos { get; }
    public int End { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
