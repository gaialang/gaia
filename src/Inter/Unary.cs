namespace Gaia.Inter;

using Gaia.Lex;
using Gaia.Symbols;

public class Unary : Op {
    public Expr expr;

    public Unary(Token tok, Expr x) : base(tok, Typing.Nil) {
        expr = x;
        Typ = Typing.Max(Typing.Int, expr.Typ);
    }

    /*
        public Expr gen() {
            return new Unary(op, expr.reduce());
        }

        public String toString() {
            return op.toString() + " " + expr.toString();
        }
        */
}
