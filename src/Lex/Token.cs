namespace Gaia.Lex {
    public class Token {
        public readonly Tag Tag;

        public Token(Tag t) {
            Tag = t;
        }

        public new string ToString() {
            return "" + (char) Tag;
        }
    }
}
