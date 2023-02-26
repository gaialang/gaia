namespace Gaia.Inter;

using Gaia.Lex;
using Gaia.Symbols;

public class Arith : Op {
    public Expr expr1, expr2;

    public Arith(Token tok, Expr x1, Expr x2) : base(tok, null) {
        expr1 = x1;
        expr2 = x2;
        Typ = Typing.Max(expr1.Typ, expr2.Typ);
        if (Typ == null) {
            // error("type error");
        }
    }

    /*
        public Expr gen() {
            return new Arith(op, expr1.reduce(), expr2.reduce());
        }

        public String toString() {
            return expr1.toString() + " " + op.toString() + " " + expr2.toString();
        }
    */
}
