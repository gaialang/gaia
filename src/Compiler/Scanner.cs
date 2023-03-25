using System.Text;

namespace Gaia.Compiler;

public class Scanner {
    public static int Line { get; private set; } = 1;
    // After reading a char, Pos will be the right number.
    public static int Column { get; private set; } = 0;
    public static int Pos { get; private set; } = 0;
    public bool IsAtEnd { get; private set; } = false;
    private TokenFlags tokenFlags;

    private readonly StreamReader source;
    private readonly Dictionary<string, TokenType> keywords = new() {
        {"package", TokenType.PackageKeyword},
        {"var", TokenType.VarKeyword},
        {"int", TokenType.IntKeyword},
        {"true", TokenType.TrueKeyword},
        {"false", TokenType.FalseKeyword},
        {"func", TokenType.FuncKeyword},
        {"return", TokenType.ReturnKeyword},
        {"import", TokenType.ImportKeyword},
        {"if", TokenType.IfKeyword},
        {"else", TokenType.ElseKeyword},
        {"while", TokenType.WhileKeyword},
        {"do", TokenType.DoKeyword},
        {"for", TokenType.ForKeyword},
        {"break", TokenType.BreakKeyword},
    };
    private readonly List<Token> tokens = new();
    // Current index of the token list.
    private int current = 0;

    public Scanner() {
        var path = Path.Combine(AppContext.BaseDirectory, "tests/test.ga");
        source = new StreamReader(path);
    }

    public bool HasPrecedingLineBreak() => (tokenFlags & TokenFlags.PrecedingLineBreak) != 0;

    public Token Scan() {
        tokenFlags = TokenFlags.None;

        SkipWhitespace();

        var ch = ReadChar();
        switch (ch) {
        case '&':
            if (MatchChar('&')) {
                return AddToken(TokenType.And, "&&");
            } else {
                return AddToken(TokenType.Ampersand, "&");
            }
        case '|':
            if (MatchChar('|')) {
                return AddToken(TokenType.Or, "||");
            } else {
                return AddToken(TokenType.Bar, "|");
            }
        case '!':
            if (MatchChar('=')) {
                return AddToken(TokenType.NotEqual, "!=");
            } else {
                return AddToken(TokenType.Not, "!");
            }
        case '"':
            return ScanString();
        case '\'':
            return ScanCharacter();
        case ',':
            return AddToken(TokenType.Comma, ",");
        case '(':
            return AddToken(TokenType.LParen, "(");
        case ')':
            return AddToken(TokenType.RParen, ")");
        case '[':
            return AddToken(TokenType.LBracket, "[");
        case ']':
            return AddToken(TokenType.RBracket, "]");
        case '{':
            return AddToken(TokenType.LBrace, "{");
        case '}':
            return AddToken(TokenType.RBrace, "}");
        case ';':
            return AddToken(TokenType.Semicolon, ";");
        case ':':
            return AddToken(TokenType.Colon, ":");
        case '+':
            return AddToken(TokenType.Plus, "+");
        case '-':
            if (MatchChar('>')) {
                return AddToken(TokenType.Arrow, "->");
            } else {
                return AddToken(TokenType.Minus, "-");
            }
        case '*':
            return AddToken(TokenType.Mul, "*");
        case '/':
            if (MatchChar('/')) {
                // Ignore comments, skip a line and re-scan.
                SkipLineComment();
                return Scan();
            } else if (MatchChar('*')) {
                SkipBlockComment();
                return Scan();
            } else {
                return AddToken(TokenType.Div, "/");
            }
        case '=':
            if (MatchChar('=')) {
                return AddToken(TokenType.EqualEqual, "==");
            } else {
                return AddToken(TokenType.Equal, "=");
            }
        case '<':
            if (MatchChar('=')) {
                return AddToken(TokenType.LessEqualThan, "<=");
            } else {
                return AddToken(TokenType.LessThan, "<");
            }
        case '>':
            if (MatchChar('=')) {
                return AddToken(TokenType.GreaterEqualThan, ">=");
            } else {
                return AddToken(TokenType.GreaterThan, ">");
            }
        case '\0':
            return AddToken(TokenType.EndOfFile, "\0");
        default:
            if (char.IsLetter(ch)) {
                return ScanIdentifier(ch);
            }
            if (char.IsDigit(ch)) {
                return ScanNumber(ch);
            }

            throw new Exception($"Near line {Scanner.Line} column {Scanner.Column}: unknown char.");
        }
    }

    private Token AddToken(TokenType t, string lexeme) {
        return new Token(t, lexeme, Pos, Line, Column);
    }

    private Token ScanNumber(char ch) {
        var b = new StringBuilder();
        b.Append(ch);
        while (char.IsDigit(PeekChar())) {
            b.Append(ReadChar());
        }

        if (PeekChar() != '.') {
            var s = b.ToString();
            return AddToken(TokenType.IntLiteral, s);
        }

        b.Append(ReadChar());
        while (char.IsDigit(PeekChar())) {
            b.Append(ReadChar());
        }

        var str = b.ToString();
        return AddToken(TokenType.FloatLiteral, str);
    }

    private Token ScanIdentifier(char ch) {
        var b = new StringBuilder();
        b.Append(ch);

        while (char.IsLetterOrDigit(PeekChar())) {
            b.Append(ReadChar());
        }

        var s = b.ToString();

        if (keywords.TryGetValue(s, out var tokenType)) {
            return AddToken(tokenType, s);
        }

        return AddToken(TokenType.Identifier, s);
    }

    private Token ScanString() {
        var b = new StringBuilder();

        while (PeekChar() != '"') {
            b.Append(ReadChar());
        }
        ReadChar();

        var s = b.ToString();

        return AddToken(TokenType.StringLiteral, s);
    }

    private Token ScanCharacter() {
        var ch = ReadChar();
        ReadChar();

        return AddToken(TokenType.CharacterLiteral, ch.ToString());
    }

    private char ReadChar() {
        var n = source.Read();
        if (n == -1) {
            IsAtEnd = true;
            return '\0';
        }
        var peek = (char)n;
        Column++;
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
                Column = 0;
                ReadChar();
                tokenFlags |= TokenFlags.PrecedingLineBreak;
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
                Column = 0;
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
        while (!IsAtEnd && PeekChar() != '\n') {
            ReadChar();
        }
    }
}
