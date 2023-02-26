namespace Gaia.Inter;

using Gaia.Symbols;

public class Set : Stmt {
    public readonly Id Id;
    public readonly Expr Expr;

    public Set(Id id, Expr expr) {
        Id = id;
        Expr = expr;
        if (Check(Id.Typ, Expr.Typ) is null) {
            Error("type error");
        }
    }

    public Typing? Check(Typing p1, Typing p2) {
        if (Typing.Numeric(p1) && Typing.Numeric(p2)) {
            return p2;
        }

        if (p1 == Typing.Bool && p2 == Typing.Bool) {
            return p2;
        }

        return null;
    }
}
