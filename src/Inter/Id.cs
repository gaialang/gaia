using Gaia.Lex;
using Gaia.Symbols;

namespace Gaia.Inter {
    public class Id : Expr {
        // Relative address
        public int Offset { get; }

        public Id(Word id, Type p, int b) : base(id, p) {
            Offset = b;
        }
    }
}
