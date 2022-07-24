namespace Gaia.Inter;

using Gaia.Symbols;

public class Else : Stmt {
    public Expr expr;
    public Stmt stmt1, stmt2;

    public Else(Expr x, Stmt s1, Stmt s2) {
        expr = x;
        stmt1 = s1;
        stmt2 = s2;
        if (expr.Typ != Typ.Bool) {
            Node.Error("boolean required in if");
        }
    }

    // public void gen(int b, int a) {
    //     var label1 = newlabel();
    //     var label2 = newlabel();
    //     expr.jumping(0, label2);
    //     emitlabel(label1);
    //     stmt1.gen(label1, a);
    //     emit("goto L" + a);
    //     emitlabel(label2);
    //     stmt2.gen(label2, a);
    // }
}
