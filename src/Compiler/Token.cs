namespace Gaia.Compiler;

public class Token {
    public TokenType Type { get; }
    public string Lexeme { get; }
    public int Line { get; }
    public int Column { get; }
    public int Pos { get; }

    public Token(TokenType type, string lexeme, int pos, int line, int col) {
        Type = type;
        Lexeme = lexeme;
        Pos = pos;
        Line = line;
        Column = col;
    }

    public override string ToString() {
        return Lexeme;
    }
}
