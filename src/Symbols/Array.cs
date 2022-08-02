namespace Gaia.Symbols;

public class Arr : Typ {
    public Typ Of;
    public int Size;

    public Arr(int sz, Typ p) : base("[]", Lex.Tag.Index, sz * p.Width) {
        Size = sz;
        Of = p;
    }

    /*
        public String toString() {
            return "[" + size + "] " + of.toString();
        }
    */
}
