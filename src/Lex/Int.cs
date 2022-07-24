namespace Gaia.Lex;

public class Int : Token {
    public readonly int Value;

    public Int(int v) : base(Lex.Tag.Int) {
        Value = v;
    }

    public override string ToString() {
        return Value.ToString();
    }
}
