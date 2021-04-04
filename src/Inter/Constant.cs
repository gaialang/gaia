using Gaia.Lex;
using Gaia.Symbols;

namespace Gaia.Inter {
    public class Constant : Expr {
        public Constant(Token tok, Type p) : base(tok, p) {
        }

        public Constant(int i) : base(new Num(i), Symbols.Type.Int) {
        }
    }
}
