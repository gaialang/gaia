namespace Gaia.Inter;

using Gaia.Lex;
using Gaia.Symbols;

public class Id : Expr {
    // Relative address
    public int Offset { get; }

    public Id(Token id, Typ p, int b) : base(id, p) {
        Offset = b;
    }
}
