using Gaia.Lex;
using Gaia.Symbols;

namespace Gaia.Inter {
    public class Expr : Node {
        public static readonly Expr Null = new(Token.Null, Type.Null);

        public readonly Token Op;
        public readonly Type Type;

        public Expr(Token tok, Type p) {
            Op = tok;
            Type = p;
        }

        public new string ToString() {
            return Op.ToString();
        }
    }
}
