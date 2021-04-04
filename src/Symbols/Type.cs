using Gaia.Lex;

namespace Gaia.Symbols {
    public class Type : Word {
        public static readonly Type Null = new("", 0, 0);
        public readonly int Width;

        public static readonly Type Int = new("int", Lex.Tag.Basic, 4);
        public static readonly Type Float32 = new("float32", Lex.Tag.Basic, 8);
        public static readonly Type Float64 = new("float64", Lex.Tag.Basic, 16);
        public static readonly Type Char = new("char", Lex.Tag.Basic, 1);
        public static readonly Type Bool = new("bool", Lex.Tag.Basic, 1);

        public Type(string s, int tag, int w) : base(s, tag) {
            Width = w;
        }

        public static bool Numeric(Type p) {
            if (p == Char || p == Int || p == Float64) {
                return true;
            }
            else {
                return false;
            }
        }

        public static Type? Max(Type p1, Type p2) {
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
