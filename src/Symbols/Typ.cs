using Gaia.Lex;

namespace Gaia.Symbols {
    public class Typ : Word {
        public readonly int Width;

        public static readonly Typ Int = new("int", Tag.Basic, 4);

        public Typ(string s, Tag tag, int w) : base(s, tag) {
            Width = w;
        }
    }
}
