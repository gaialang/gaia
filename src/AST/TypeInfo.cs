namespace Gaia.AST;

public class TypeInfo {
    public static readonly TypeInfo Package = new TypeInfo(TypeKind.Package), Func = new TypeInfo(TypeKind.Func),
    Int = new TypeInfo(TypeKind.Int);

    public TypeKind Kind { get; }

    public TypeInfo(TypeKind t) {
        Kind = t;
    }
}
