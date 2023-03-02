namespace Gaia.Lex;

// Reserved words
public class Word : Token {
    public readonly string Lexeme;

    public static readonly Word And = new Word("&&", Lex.Tag.And),
        Or = new Word("||", Lex.Tag.Or),
        Eq = new Word("==", Lex.Tag.Eq),
        Ne = new Word("!=", Lex.Tag.Ne),
        Le = new Word("<=", Lex.Tag.Le),
        Ge = new Word(">=", Lex.Tag.Ge),
        Minus = new Word("minus", Lex.Tag.Minus), // minus is different from subtraction.
        True = new Word("true", Lex.Tag.True),
        False = new Word("false", Lex.Tag.False),
        Temp = new Word("t", Lex.Tag.Temp);

    public Word(string s, int tag) : base(tag) {
        Lexeme = s;
    }

    public override string ToString() {
        return Lexeme;
    }
}
