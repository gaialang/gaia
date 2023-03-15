using System.Text;

namespace Gaia.Compiler;

public class Scanner {
    public static int Line { get; private set; } = 1;

    // After reading a char, Pos will be the right number.
    public static int Pos { get; private set; } = 0;

    private StreamReader source;
    private bool isAtEnd = false;
    private readonly Dictionary<string, TokenType> keywords = new() {
        {"package", TokenType.Package},
        {"var", TokenType.Var},
        {"int", TokenType.Int},
        {"true", TokenType.True},
        {"false", TokenType.False},
        {"func", TokenType.Func},
    };

    public Scanner() {
        source = new StreamReader(AppContext.BaseDirectory + "tests/test.ga");
    }

    public Token Scan() {
        SkipWhitespace();

        var ch = Advance();
        switch (ch) {
        /*
        case '&':
            if (Readch('&')) {
                return Word.And;
            } else {
                return new Token('&');
            }
        case '|':
            if (Readch('|')) {
                return Word.Or;
            } else {
                return new Token('|');
            }
        case '!':
            if (Readch('=')) {
                return Word.Ne;
            } else {
                return new Token('!');
            }
            */
        case ',':
            return new Token(TokenType.Comma, ",", Line, Pos);
        case '(':
            return new Token(TokenType.LParen, "(", Line, Pos);
        case ')':
            return new Token(TokenType.RParen, ")", Line, Pos);
        case '{':
            return new Token(TokenType.LBrace, "{", Line, Pos);
        case '}':
            return new Token(TokenType.RBrace, "}", Line, Pos);
        case ';':
            return new Token(TokenType.Semicolon, ";", Line, Pos);
        case ':':
            return new Token(TokenType.Colon, ":", Line, Pos);
        case '*':
            return new Token(TokenType.Multiply, "*", Line, Pos);
        case '+':
            return new Token(TokenType.Plus, "+", Line, Pos);
        case '-':
            if (Peek() == '>') {
                Advance();
                return new Token(TokenType.Arrow, "->", Line, Pos);
            } else {
                return new Token(TokenType.Minus, "-", Line, Pos);
            }
        case '=':
            if (Peek() == '=') {
                Advance();
                return new Token(TokenType.EqualEqual, "==", Line, Pos);
            } else {
                return new Token(TokenType.Equal, "=", Line, Pos);
            }
        case '/':
            if (Peek() == '/') {
                Advance();
                // Ignore comments, skip a line and re-scan.
                SkipLineComment();
                return Scan();
            } else if (Peek() == '*') {
                Advance();
                SkipBlockComment();
                return Scan();
            } else {
                return new Token(TokenType.Divide, "/", Line, Pos);
            }
        case '<':
            if (Peek() == '=') {
                return new Token(TokenType.LessEqualThan, "<=", Line, Pos);
            } else {
                return new Token(TokenType.LessThan, "<", Line, Pos);
            }
        case '>':
            if (Peek() == '=') {
                return new Token(TokenType.GreaterEqualThan, ">=", Line, Pos);
            } else {
                return new Token(TokenType.GreaterThan, ">", Line, Pos);
            }
        case '\0':
            return new Token(TokenType.EndOfFile, "\0", Line, Pos);
        default:
            if (char.IsLetter(ch)) {
                return Identifier(ch);
            }
            if (char.IsDigit(ch)) {
                return Number(ch);
            }

            throw new Exception($"Near line {Scanner.Line} position {Scanner.Pos}: unknown char.");
        }
    }

    private Token Number(char ch) {
        var b = new StringBuilder();
        b.Append(ch);
        while (char.IsDigit(Peek())) {
            b.Append(Advance());
        }

        if (Peek() != '.') {
            var s = b.ToString();
            var v = int.Parse(s);
            return new Token(TokenType.IntLiteral, s, Line, Pos, v);
        }

        b.Append(Advance());
        while (char.IsDigit(Peek())) {
            b.Append(Advance());
        }

        var str = b.ToString();
        var d = double.Parse(str);
        return new Token(TokenType.FloatLiteral, str, Line, Pos, d);
    }

    private Token Identifier(char ch) {
        var b = new StringBuilder();
        b.Append(ch);

        while (char.IsLetterOrDigit(Peek())) {
            b.Append(Advance());
        }

        var s = b.ToString();

        if (keywords.TryGetValue(s, out var tokenType)) {
            return new Token(tokenType, s, Line, Pos);
        }

        return new Token(TokenType.Id, s, Line, Pos);
    }

    private char Advance() {
        var n = source.Read();
        if (n == -1) {
            isAtEnd = true;
            return '\0';
        }
        var peek = (char)n;
        Pos++;
        return peek;
    }

    private char Peek() {
        var n = source.Peek();
        if (n == -1) {
            return '\0';
        }
        var peek = (char)n;
        return peek;
    }

    private void SkipWhitespace() {
        while (!isAtEnd) {
            var c = Peek();
            switch (c) {
            case ' ':
            case '\r':
            case '\t':
                Advance();
                break;
            case '\n':
                Line++;
                Pos = 0;
                Advance();
                break;
            default:
                return;
            }
        }
    }

    private void SkipBlockComment() {
        while (!isAtEnd) {
            var c = Peek();
            switch (c) {
            case ' ':
            case '\r':
            case '\t':
                Advance();
                break;
            case '\n':
                Line++;
                Pos = 0;
                Advance();
                break;
            case '*':
                Advance();
                if (Peek() == '/') {
                    Advance();
                    // Reach the end of the block comment.
                    return;
                }
                break;
            default:
                Advance();
                break;
            }
        }
    }

    private void SkipLineComment() {
        while (!isAtEnd && Peek() != '\n' && Peek() != '\r') {
            Advance();
        }
    }
}
