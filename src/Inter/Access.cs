namespace Gaia.Inter;

using Gaia.Lex;
using Gaia.Symbols;

public class Access : Op {
    public Id array;
    public Expr index;

    public Access(Id a, Expr i, Typing p) : base(new Word("[]", Tag.Index), p) {
        array = a;
        index = i;
    }

    /*
        public Expr gen() {
            return new Access(array, index.reduce(), type);
        }

        public void jumping(int t, int f) {
            emitjumps(reduce().toString(), t, f);
        }

        public String toString() {
            return array.toString() + " [ " + index.toString() + " ]";
        }
        */
}
