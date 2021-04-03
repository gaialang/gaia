using Gaia.Lex;
using Gaia.Symbols;

namespace Gaia.Inter {
    public class Expr : Node {
        public readonly Token Op;
        public readonly Typ Type;

        public Expr(Token tok, Typ p) {
            Op = tok;
            Type = p;
        }

        public new string ToString() {
            return Op.ToString();
        }
    }
}
