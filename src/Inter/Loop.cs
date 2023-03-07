namespace Gaia.Inter;

using Gaia.Symbols;

public class Loop : Stmt {
    private Expr? expr;
    private Stmt stmt;

    public Loop() {
        expr = null;
        stmt = Stmt.Null;
    }

    public void Init(Stmt s, Expr x) {
        expr = x;
        stmt = s;
        if (expr.Typ != Typing.Bool) {
            Error("boolean required in loop");
        }
    }

    /*
    public void gen(int b, int a) {
        after = a;
        var label = newlabel();
        stmt.gen(b, label);
        emitlabel(label);
        expr.jumping(b, 0);
    }
    */
}
