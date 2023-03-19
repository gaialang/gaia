using System.Text;

namespace Gaia.Compiler;

public class Scanner {
    public static int Line { get; private set; } = 1;
    // After reading a char, Pos will be the right number.
    public static int Pos { get; private set; } = 0;
    public bool IsAtEnd { get; private set; } = false;

    private readonly StreamReader source;
    private readonly Dictionary<string, TokenType> keywords = new() {
        {"package", TokenType.PackageKeyword},
        {"var", TokenType.VarKeyword},
        {"int", TokenType.IntKeyword},
        {"true", TokenType.TrueKeyword},
        {"false", TokenType.FalseKeyword},
        {"func", TokenType.FuncKeyword},
        {"return", TokenType.ReturnKeyword},
    };
    private readonly List<Token> tokens = new();
    // Current index of the token list.
    private int current = 0;

    public Scanner() {
        source = new StreamReader(AppContext.BaseDirectory + "tests/test.ga");
    }

    public Token Scan() {
        SkipWhitespace();

        var ch = ReadChar();
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
            return new Token(TokenType.Mul, "*", Line, Pos);
        case '+':
            return new Token(TokenType.Plus, "+", Line, Pos);
        case '-':
            if (MatchChar('>')) {
                return new Token(TokenType.Arrow, "->", Line, Pos);
            } else {
                return new Token(TokenType.Minus, "-", Line, Pos);
            }
        case '=':
            if (MatchChar('=')) {
                return new Token(TokenType.EqualEqual, "==", Line, Pos);
            } else {
                return new Token(TokenType.Equal, "=", Line, Pos);
            }
        case '/':
            if (MatchChar('/')) {
                // Ignore comments, skip a line and re-scan.
                SkipLineComment();
                return Scan();
            } else if (MatchChar('*')) {
                SkipBlockComment();
                return Scan();
            } else {
                return new Token(TokenType.Div, "/", Line, Pos);
            }
        case '<':
            if (MatchChar('=')) {
                return new Token(TokenType.LessEqualThan, "<=", Line, Pos);
            } else {
                return new Token(TokenType.LessThan, "<", Line, Pos);
            }
        case '>':
            if (MatchChar('=')) {
                return new Token(TokenType.GreaterEqualThan, ">=", Line, Pos);
            } else {
                return new Token(TokenType.GreaterThan, ">", Line, Pos);
            }
        case '\0':
            return new Token(TokenType.EndOfFile, "\0", Line, Pos);
        default:
            if (char.IsLetter(ch)) {
                return ScanIdentifier(ch);
            }
            if (char.IsDigit(ch)) {
                return ScanNumber(ch);
            }

            throw new Exception($"Near line {Scanner.Line} position {Scanner.Pos}: unknown char.");
        }
    }

    private Token ScanNumber(char ch) {
        var b = new StringBuilder();
        b.Append(ch);
        while (char.IsDigit(PeekChar())) {
            b.Append(ReadChar());
        }

        if (PeekChar() != '.') {
            var s = b.ToString();
            var v = int.Parse(s);
            return new Token(TokenType.IntLiteral, s, Line, Pos, v);
        }

        b.Append(ReadChar());
        while (char.IsDigit(PeekChar())) {
            b.Append(ReadChar());
        }

        var str = b.ToString();
        var d = double.Parse(str);
        return new Token(TokenType.FloatLiteral, str, Line, Pos, d);
    }

    private Token ScanIdentifier(char ch) {
        var b = new StringBuilder();
        b.Append(ch);

        while (char.IsLetterOrDigit(PeekChar())) {
            b.Append(ReadChar());
        }

        var s = b.ToString();

        if (keywords.TryGetValue(s, out var tokenType)) {
            return new Token(tokenType, s, Line, Pos);
        }

        return new Token(TokenType.Identifier, s, Line, Pos);
    }

    private char ReadChar() {
        var n = source.Read();
        if (n == -1) {
            IsAtEnd = true;
            return '\0';
        }
        var peek = (char)n;
        Pos++;
        return peek;
    }

    private bool MatchChar(char c) {
        if (PeekChar() == c) {
            ReadChar();
            return true;
        } else {
            return false;
        }

    }

    private char PeekChar() {
        var n = source.Peek();
        if (n == -1) {
            return '\0';
        }
        var peek = (char)n;
        return peek;
    }

    private void SkipWhitespace() {
        while (!IsAtEnd) {
            var c = PeekChar();
            switch (c) {
            case ' ':
            case '\r':
            case '\t':
                ReadChar();
                break;
            case '\n':
                Line++;
                Pos = 0;
                ReadChar();
                break;
            default:
                return;
            }
        }
    }

    private void SkipBlockComment() {
        while (!IsAtEnd) {
            var c = PeekChar();
            switch (c) {
            case ' ':
            case '\r':
            case '\t':
                ReadChar();
                break;
            case '\n':
                Line++;
                Pos = 0;
                ReadChar();
                break;
            case '*':
                ReadChar();
                if (MatchChar('/')) {
                    // Reach the end of the block comment.
                    return;
                }
                break;
            default:
                ReadChar();
                break;
            }
        }
    }

    private void SkipLineComment() {
        while (!IsAtEnd && PeekChar() != '\n' && PeekChar() != '\r') {
            ReadChar();
        }
    }
}
