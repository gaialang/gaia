namespace Gaia.Lex {
    public class Token {
        public static readonly Token Null = new(0);
        public readonly int Tag;

        public Token(int t) {
            Tag = t;
        }

        public override string ToString() {
            return Tag.ToString();
        }
    }
}
