namespace Gaia.Inter;

using Gaia.Lex;
using Gaia.Symbols;

public class Expr : Node {
    public Token Op;
    public Typ Typ;

    public Expr(Token tok, Typ p) {
        Op = tok;
        Typ = p;
    }

    public override string ToString() {
        return Op.ToString();
    }
}
