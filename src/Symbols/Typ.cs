using Gaia.Lex;

namespace Gaia.Symbols {
    public class Typ : Word {
        public static readonly Typ Null = new("", 0, 0);
        public readonly int Width;

        public static readonly Typ Int = new("int", Lex.Tag.Int, 4);
        public static readonly Typ Float32 = new("float32", Lex.Tag.Float32, 8);
        public static readonly Typ Float64 = new("float64", Lex.Tag.Float64, 16);
        public static readonly Typ Char = new("char", Lex.Tag.Char, 1);
        public static readonly Typ Bool = new("bool", Lex.Tag.Bool, 1);
        public static readonly Typ Pkg = new("package", Lex.Tag.Pkg, 4);
        public static readonly Typ Func = new("func", Lex.Tag.Func, 4);

        public Typ(string s, int tag, int w) : base(s, tag) {
            Width = w;
        }

        public static bool Numeric(Typ p) {
            if (p == Char || p == Int || p == Float64) {
                return true;
            }
            else {
                return false;
            }
        }

        public static Typ? Max(Typ p1, Typ p2) {
            if (!Numeric(p1) || !Numeric(p2)) {
                return null;
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
}
