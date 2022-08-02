namespace Gaia.Inter;

using Gaia.Lex;
using Gaia.Symbols;

public class Rel : Logical {
    public Rel(Token tok, Expr x1, Expr x2) : base(tok, x1, x2) {
    }

    public Typ? check(Typ p1, Typ p2) {
        /*
        if (p1 instanceof Array || p2 instanceof Array) {
            return null;
        }
        */
        if (p1 == p2) {
            return Typ.Bool;
        }
        else {
            return null;
        }
    }

    /*
        public void jumping(int t, int f) {
            var a = expr1.reduce();
            var b = expr2.reduce();

            var test = a.toString() + " " + op.toString() + " " + b.toString();
            emitjumps(test, t, f);
        }
        */
}
