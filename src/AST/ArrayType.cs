namespace Gaia.AST;

public class ArrayType : TypeInfo {
    public TypeInfo ElementType { get; }

    public ArrayType(TypeInfo elem) : base(TypeKind.ArrayType) {
        ElementType = elem;
    }
}
