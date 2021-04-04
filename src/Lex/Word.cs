namespace Gaia.Lex {
    public class Word : Token {
        public readonly string Lexeme;

        public static readonly Word Var = new("var", Lex.Tag.Var);
        public static readonly Word Package = new("package", Lex.Tag.Package);

        public Word(string s, int tag) : base(tag) {
            Lexeme = s;
        }

        public override string ToString() {
            return Lexeme;
        }
    }
}
