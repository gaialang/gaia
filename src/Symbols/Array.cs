namespace Gaia.Symbols;

public class Arr : Typing {
    public Typing Of;
    public int Size;

    public Arr(int sz, Typing p) : base("[]", Lex.Tag.Index, sz * p.Width) {
        Size = sz;
        Of = p;
    }

    /*
        public String toString() {
            return "[" + size + "] " + of.toString();
        }
    */
}
