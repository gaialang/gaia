namespace Gaia.Inter;

using Gaia.Lex;
using Gaia.Symbols;

public class Op : Expr {
    public Op(Token tok, Typing p) : base(tok, p) {
    }

    /*
        public Expr reduce() {
            var x = gen();
            var t = new Temp(type);
            emit(t + " = " + x.toString());
            return t;
        }
    */
}
