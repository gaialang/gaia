namespace Gaia.Lex {
    public class Word : Token {
        public readonly string Lexeme;
        public static readonly Word Var = new("var", Tag.Var);

        public Word(string s, Tag tag) : base(tag) {
            Lexeme = s;
        }

        public new string ToString() {
            return Lexeme;
        }
    }
}
