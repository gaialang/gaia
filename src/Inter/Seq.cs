namespace Gaia.Inter {
    public class Seq : Stmt {
        public readonly Stmt Stmt1;
        public readonly Stmt Stmt2;

        public Seq(Stmt s1, Stmt s2) {
            Stmt1 = s1;
            Stmt2 = s2;
        }
    }
}
