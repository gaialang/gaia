namespace Gaia.Lex;

public class Token {
    public readonly int Tag;

    public Token(int t) {
        Tag = t;
    }

    public override string ToString() {
        return Tag.ToString();
    }
}
