using Gaia.Lex;
using Gaia.Symbols;

namespace Gaia.Inter {
    public class Constant : Expr {
        public Constant(Token tok, Typ p) : base(tok, p) {
        }

        public Constant(int i) : base(new Int(i), Typ.Int) {
        }
    }
}
