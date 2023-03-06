namespace Gaia.Inter;

using Gaia.Lex;
using Gaia.Symbols;

public class Constant : Expr {
    public static readonly Constant True = new(Word.True, Typing.Bool),
        False = new(Word.False, Typing.Bool);

    public Constant(Token tok, Typing p) : base(tok, p) {
    }

    public Constant(int i) : base(new Num(i), Typing.Int) {
    }

    /*
        public void jumping(int t, int f) {
        if (this == True && t != 0) {
            emit("goto L" + t);
        }
        else if (this == False && f != 0) {
            emit("goto L" + f);
        }
    }
    */
}
