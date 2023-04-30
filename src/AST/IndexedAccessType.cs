using Gaia.Domain;

namespace Gaia.AST;

public class IndexedAccessType : BaseNode {
    public Expression ObjectType { get; }
    /// <summary>
    /// Empty string means size not specified.
    /// </summary>
    public string IndexType { get; }

    public IndexedAccessType(Expression obj, string index, int pos, int end) : base(SyntaxKind.IndexedAccessType, pos, end) {
        ObjectType = obj;
        IndexType = index;
    }
}
