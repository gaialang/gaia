
namespace Gaia.Inter;

using Gaia.Lex;

public class Not : Logical {
    public Not(Token tok, Expr x2) : base(tok, x2, x2) {
    }

    /*
        public void jumping(int t, int f) {
            expr2.jumping(f, t);
        }

        public String toString() {
            return op.toString() + " " + expr2.toString();
        }
    */
}
