namespace Gaia.Compiler;

public enum TokenType {
    Package,
    Id,
    EndOfFile,
    Binary, // binary operators + - * /
    Semicolon,
    Var,
    Colon,
    Int,
    Equal,
    EqualEqual,
    IntLiteral,
    FloatLiteral,

    /*
        FUNC,
        IF,
        THEN,
        ELSE,
        FOR,
        IN,
        RPAREN,
        LPAREN,
        COMMA,
        UNARY,
        PLUS,
        MINUS,
        LESS
    */
}
