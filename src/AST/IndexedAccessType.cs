namespace Gaia.AST;

public class IndexedAccessType : TypeInfo {
    public TypeInfo ObjectType { get; }
    /// <summary>
    /// Empty string means size not specified.
    /// </summary>
    public string IndexType { get; }

    public IndexedAccessType(TypeInfo obj, string index) : base(TypeKind.IndexedAccessType) {
        ObjectType = obj;
        IndexType = index;
    }
}
