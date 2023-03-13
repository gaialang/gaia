namespace Gaia.Compiler;

public class Token {
    public TokenType Type { get; }
    public string Lexeme { get; }
    public int Line { get; }
    public int Pos { get; }
    public object? Value { get; }

    public Token(TokenType type, string lexeme, int line, int pos, object? value = null) {
        Type = type;
        Lexeme = lexeme;
        Line = line;
        Pos = pos;
        Value = value;
    }

    public override string ToString() {
        return Lexeme;
    }
}
