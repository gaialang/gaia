namespace Gaia.Symbols;

using Gaia.Lex;

public class Typing : Word {
    public readonly int Width;

    public static readonly Typing Int = new("int", Lex.Tag.Basic, 4),
        Float32 = new("float32", Lex.Tag.Basic, 8),
        Float64 = new("float64", Lex.Tag.Basic, 16),
        Char = new("char", Lex.Tag.Basic, 1),
        Bool = new("bool", Lex.Tag.Basic, 1),
        Pkg = new("package", Lex.Tag.Pkg, 4),
        Nil = new("nil", Lex.Tag.Nil, 1);

    // TODO: should func have a width?
    public static readonly Typing Func = new("func", Lex.Tag.Func, 4);

    public Typing(string s, int tag, int w) : base(s, tag) {
        Width = w;
    }

    public static bool Numeric(Typing p) {
        if (p == Char || p == Int || p == Float64) {
            return true;
        } else {
            return false;
        }
    }

    public static Typing Max(Typing p1, Typing p2) {
        if (!Numeric(p1) || !Numeric(p2)) {
            throw new Exception("type mismatched");
        }

        if (p1 == Float64 || p2 == Float64) {
            return Float64;
        }

        if (p1 == Int || p2 == Int) {
            return Int;
        }

        return Char;
    }
}
