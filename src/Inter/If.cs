namespace Gaia.Inter;

using Gaia.Symbols;

public class If : Stmt {
    public Expr Expr;
    public Stmt Stmt;

    public If(Expr x, Stmt s) {
        Expr = x;
        Stmt = s;
        if (Expr.Typ != Typing.Bool) {
            Node.Error("boolean required in if");
        }
    }

    // public void gen(int b, int a) {
    //     var label = newlabel();
    //     expr.jumping(0, a);
    //     emitlabel(label);
    //     stmt.gen(label, a);
    // }
}
