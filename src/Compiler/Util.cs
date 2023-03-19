using Gaia.AST;

namespace Gaia.Compiler;

public static class Util {
    public static string ConvertType(IdType t) {
        switch (t) {
        case IdType.Int:
            return "int";
        default:
            return "";
        }
    }
}
