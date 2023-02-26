namespace Gaia.Lex;

public class Float64 : Token {
    public readonly double Value;

    public Float64(double v) : base(Lex.Tag.Real) {
        Value = v;
    }

    public override string ToString() {
        return Value.ToString();
    }
}
