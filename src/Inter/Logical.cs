namespace Gaia.Inter;

using Gaia.Lex;
using Gaia.Symbols;

public class Logical : Expr {
    public Expr expr1;
    public Expr expr2;

    public Logical(Token tok, Expr x1, Expr x2) : base(tok, null) {
        expr1 = x1;
        expr2 = x2;
        Typ = Check(expr1.Typ, expr2.Typ);
        if (Typ == null) {
            Error("type error");
        }
    }

    public Typing Check(Typing p1, Typing p2) {
        if (p1 == Typing.Bool && p2 == Typing.Bool) {
            return Typing.Bool;
        } else {
            return null;
        }
    }

    /*
        public Expr gen() {
            var f = newlabel();
            var a = newlabel();
            var temp = new Temp(type);
            jumping(0, f);
            emit(temp + " = true");
            emit("goto L" + a);
            emitlabel(f);
            emit(temp + " = false");
            emitlabel(a);
            return temp;
        }

        public String toString() {
            return expr1.toString() + " " + op.toString() + " " + expr2.toString();
        } */
}
