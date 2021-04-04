using Gaia.Symbols;

namespace Gaia.Inter {
    public class Set : Stmt {
        public readonly Id Id;
        public readonly Expr Expr;

        public Set(Id i, Expr x) {
            Id = i;
            Expr = x;
            if (Check(Id.Type, Expr.Type) is null) {
                Error("type error");
            }
        }

        public Type? Check(Type p1, Type p2) {
            if (Type.Numeric(p1) && Type.Numeric(p2)) {
                return p2;
            }

            if (p1 == Type.Bool && p2 == Type.Bool) {
                return p2;
            }

            return null;
        }
    }
}
