namespace Gaia.Lex {
    public class Word : Token {
        public readonly string Lexeme;
        public static readonly Word Var = new("var", Lex.Tag.Var);

        public Word(string s, int tag) : base(tag) {
            Lexeme = s;
        }

        public new string ToString() {
            return Lexeme;
        }
    }
}
