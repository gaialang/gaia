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
    True,
    False,
    Func,
    LParen,
    RParen,
    Comma,
    LBrace,
    RBrace,
    Arrow,
    Minus,

    /*
        IF,
        THEN,
        ELSE,
        FOR,
        IN,
        UNARY,
        PLUS,
        MINUS,
        LESS
    */
}
