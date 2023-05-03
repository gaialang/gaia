using System.Text;
using Gaia.Domain;
using static Gaia.Domain.SyntaxKindText;

namespace Gaia.Compiler;

public class Scanner {
    public static int Line { get; private set; } = 1;
    // After reading a char, Pos will be the right number.
    public static int Column { get; private set; } = 0;

    public static int Pos { get; private set; } = 0;

    // Start position of whitespace before current token
    public int FullStartPos { get; private set; } = 0;
    public int TokenStart { get; private set; } = 0;
    public int TokenEnd => Pos;
    private string text = "";

    public bool IsAtEnd { get; private set; } = false;
    private SyntaxKind token = SyntaxKind.Unknown;
    public TokenFlags TokenFlags { get; private set; } = TokenFlags.None;
    public string TokenValue { get; private set; } = "";

    private readonly StreamReader source;

    // Current index of the token list.
    private int current = 0;

    public Scanner() {
        var path = Path.Combine(AppContext.BaseDirectory, "tests/test.ga");
        source = new StreamReader(path);
    }

    public bool HasPrecedingLineBreak() => (TokenFlags & TokenFlags.PrecedingLineBreak) != 0;

    public SyntaxKind Scan() {
        FullStartPos = Pos;
        TokenFlags = TokenFlags.None;

        SkipWhitespace();

        TokenStart = Pos;
        var ch = ReadChar();
        switch (ch) {
        case '&':
            if (MatchChar('&')) {
                return AddToken(SyntaxKind.AmpersandAmpersandToken);
            } else {
                return AddToken(SyntaxKind.AmpersandToken);
            }
        case '|':
            if (MatchChar('|')) {
                return AddToken(SyntaxKind.BarBarToken);
            } else {
                return AddToken(SyntaxKind.BarToken);
            }
        case '!':
            if (MatchChar('=')) {
                return AddToken(SyntaxKind.ExclamationEqualsToken);
            } else {
                return AddToken(SyntaxKind.ExclamationToken);
            }
        case '"':
            return AddToken(SyntaxKind.StringLiteral, ScanString(ch));
        case '\'':
            return AddToken(SyntaxKind.CharacterLiteral, ScanCharacter(ch));
        case ',':
            return AddToken(SyntaxKind.CommaToken);
        case '(':
            return AddToken(SyntaxKind.OpenParenToken);
        case ')':
            return AddToken(SyntaxKind.CloseParenToken);
        case '[':
            return AddToken(SyntaxKind.OpenBracketToken);
        case ']':
            return AddToken(SyntaxKind.CloseBracketToken);
        case '{':
            return AddToken(SyntaxKind.OpenBraceToken);
        case '}':
            return AddToken(SyntaxKind.CloseBraceToken);
        case ';':
            return AddToken(SyntaxKind.SemicolonToken);
        case ':':
            return AddToken(SyntaxKind.ColonToken);
        case '+':
            return AddToken(SyntaxKind.PlusToken);
        case '-':
            if (MatchChar('>')) {
                return AddToken(SyntaxKind.MinusGreaterThanToken);
            } else {
                return AddToken(SyntaxKind.MinusToken);
            }
        case '*':
            return AddToken(SyntaxKind.AsteriskToken);
        case '/':
            if (MatchChar('/')) {
                // Ignore comments, skip a line and re-scan.
                SkipLineComment();
                return Scan();
            } else if (MatchChar('*')) {
                SkipBlockComment();
                return Scan();
            } else {
                return AddToken(SyntaxKind.SlashToken);
            }
        case '=':
            if (MatchChar('=')) {
                return AddToken(SyntaxKind.EqualsEqualsToken);
            } else {
                return AddToken(SyntaxKind.EqualsToken);
            }
        case '<':
            if (MatchChar('=')) {
                return AddToken(SyntaxKind.LessThanEqualsToken);
            } else {
                return AddToken(SyntaxKind.LessThanToken);
            }
        case '>':
            if (MatchChar('=')) {
                return AddToken(SyntaxKind.GreaterThanEqualsToken);
            } else {
                return AddToken(SyntaxKind.GreaterThanToken);
            }
        case '\0':
            return AddToken(SyntaxKind.EndOfFileToken);
        default:
            if (char.IsLetter(ch)) {
                return ScanIdentifier(ch);
            }
            if (char.IsDigit(ch)) {
                return ScanNumber(ch);
            }

            return SyntaxKind.Unknown;
        }
    }

    private SyntaxKind AddToken(SyntaxKind kind) {
        return token = kind;
    }

    private SyntaxKind AddToken(SyntaxKind kind, string value) {
        token = kind;
        TokenValue = value;
        return token;
    }

    private SyntaxKind ScanNumber(char ch) {
        var b = new StringBuilder();
        b.Append(ch);
        while (char.IsDigit(PeekChar())) {
            b.Append(ReadChar());
        }

        if (PeekChar() != '.') {
            var numInt = b.ToString();
            return AddToken(SyntaxKind.IntLiteral, numInt);
        }

        b.Append(ReadChar());
        while (char.IsDigit(PeekChar())) {
            b.Append(ReadChar());
        }

        var numFloat = b.ToString();
        return AddToken(SyntaxKind.FloatLiteral, numFloat);
    }

    private SyntaxKind ScanIdentifier(char ch) {
        var b = new StringBuilder();
        b.Append(ch);

        while (char.IsLetterOrDigit(PeekChar())) {
            b.Append(ReadChar());
        }

        var s = b.ToString();

        if (TextToKeyword.TryGetValue(s, out var kind)) {
            return AddToken(kind);
        }

        return AddToken(SyntaxKind.Identifier, s);
    }

    private string ScanString(char ch) {
        var b = new StringBuilder();
        b.Append(ch);

        while (PeekChar() != '"') {
            b.Append(ReadChar());
        }
        b.Append(ReadChar());

        var s = b.ToString();

        return s;
    }

    private string ScanCharacter(char first) {
        var b = new StringBuilder();
        b.Append(first);

        if (PeekChar() != '\'') {
            b.Append(ReadChar());
            b.Append(ReadChar());
        } else {
            b.Append(ReadChar());
        }

        return b.ToString();
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
                TokenFlags |= TokenFlags.PrecedingLineBreak;
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
