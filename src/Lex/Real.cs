namespace Gaia.Lex;

public class Real : Token {
    public readonly double Value;

    public Real(double v) : base(Lex.Tag.Real) {
        Value = v;
    }

    public override string ToString() {
        return Value.ToString();
    }
}
