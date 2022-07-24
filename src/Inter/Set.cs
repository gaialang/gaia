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

    public Typ? Check(Typ p1, Typ p2) {
        if (Typ.Numeric(p1) && Typ.Numeric(p2)) {
            return p2;
        }

        if (p1 == Typ.Bool && p2 == Typ.Bool) {
            return p2;
        }

        return null;
    }
}
