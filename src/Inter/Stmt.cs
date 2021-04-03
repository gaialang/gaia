namespace Gaia.Inter {
    public class Stmt : Node {
        public static readonly Stmt Null = new();

        public int After { get; } = 0;

        public static readonly Stmt Enclosing = Null;
    }
}
