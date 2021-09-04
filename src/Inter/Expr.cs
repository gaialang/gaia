using Gaia.Lex;
using Gaia.Symbols;

namespace Gaia.Inter {
    public class Expr : Node {
        public static readonly Expr Null = new(Token.Null, Typ.Null);

        public readonly Token Op;
        public readonly Typ Typ;

        public Expr(Token tok, Typ p) {
            Op = tok;
            Typ = p;
        }

        public override string ToString() {
            return Op.ToString();
        }
    }
}
