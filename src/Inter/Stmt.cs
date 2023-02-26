namespace Gaia.Inter;

public class Stmt : Node {
    public static Stmt Null = new Stmt();
    public int After { get; } = 0;
    public static Stmt Enclosing = Null;

    public void gen(int b, int a) {
    }
}
