namespace Gaia.Inter;

using Gaia.Symbols;

public class While : Stmt {
    private Expr? Expr;
    private Stmt Stmt;

    public While() {
        Expr = null;
        Stmt = Stmt.Null;
    }

    public void Init(Expr x, Stmt s) {
        Expr = x;
        Stmt = s;
        if (Expr.Typ != Typing.Bool) {
            Node.Error("boolean required in while");
        }
    }

    /*
    public void gen(int b, int a) {
        after = a;
        expr.jumping(0, a);
        var label = newlabel();
        emitlabel(label);
        stmt.gen(label, b);
        emit("goto L" + b);
    }
    */
}
