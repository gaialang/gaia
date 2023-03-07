namespace Gaia.Inter;

public class Break : Stmt {
    private Stmt Stmt;

    public Break() {
        if (Stmt.Enclosing == Stmt.Null) {
            Error("unenclosed break");
        }
        Stmt = Enclosing;
    }

    /*
    public void gen(int b, int a) {
        emit("goto L" + stmt.after);
    }
    */
}
