namespace Gaia.Inter;

using Gaia.Lex;
using Gaia.Symbols;

public class Rel : Logical {
    public Rel(Token tok, Expr x1, Expr x2) : base(tok, x1, x2) {
    }

    public new Typing Check(Typing p1, Typing p2) {
        if (p1 == p2) {
            return Typing.Bool;
        } else {
            return Typing.Nil;
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
