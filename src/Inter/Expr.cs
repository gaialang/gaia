namespace Gaia.Inter;

using Gaia.Lex;
using Gaia.Symbols;

public class Expr : Node {
    public Token Op;
    public Typing Typ;

    public Expr(Token tok, Typing p) {
        Op = tok;
        Typ = p;
    }

    public override string ToString() {
        return Op.ToString();
    }
}
