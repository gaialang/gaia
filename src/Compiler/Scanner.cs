using System.Text;
using Gaia.AST;
using Gaia.Domain;
using System.Diagnostics;
using static Gaia.Domain.SyntaxKindText;

namespace Gaia.Compiler;

public record LineAndCharacter(int Line, int Character);

public class Scanner {
    private int pos = 0;
    private int end = 0;

    // Start position of whitespace before current token
    public int FullStartPos { get; private set; } = 0;

    public int TokenStart { get; private set; } = 0;
    public int TokenEnd => pos;
    private string text = "";
    private SourceFile sourceFile;

    private SyntaxKind token = SyntaxKind.Unknown;
    public TokenFlags TokenFlags { get; private set; } = TokenFlags.None;
    public string TokenValue { get; private set; } = "";

    public Scanner() {
        sourceFile = new SourceFile();
        setText(sourceFile.Text);
    }

    public void setText(string newText) {
        text = newText;
        end = text.Length;
    }

    public string LineColumn(int pos) {
        var lineAndCharacter = getLineAndCharacterOfPosition(pos);
        return $"{lineAndCharacter.Line + 1},{lineAndCharacter.Character + 1}";
    }

    public List<int> computeLineStarts(string text) {
        var result = new List<int>();
        var pos = 0;
        var lineStart = 0;
        while (pos < text.Length) {
            var ch = (CharacterCodes)text[pos];
            pos++;
            switch (ch) {
            case CharacterCodes.carriageReturn:
                if ((CharacterCodes)text[pos] == CharacterCodes.lineFeed) {
                    pos++;
                }
                // falls through
                goto case CharacterCodes.lineFeed;
            case CharacterCodes.lineFeed:
                result.Add(lineStart);
                lineStart = pos;
                break;
            default:
                if (ch > CharacterCodes.maxAsciiCharacter && isLineBreak(ch)) {
                    result.Add(lineStart);
                    lineStart = pos;
                }
                break;
            }
        }
        result.Add(lineStart);
        return result;
    }

    public bool isWhiteSpaceLike(CharacterCodes ch) {
        return isWhiteSpaceSingleLine(ch) || isLineBreak(ch);
    }

    /** Does not include line breaks. For that, see isWhiteSpaceLike. */
    public bool isWhiteSpaceSingleLine(CharacterCodes ch) {
        // Note: nextLine is in the Zs space, and should be considered to be a whitespace.
        // It is explicitly not a line-break as it isn't in the exact set specified by EcmaScript.
        return ch == CharacterCodes.space ||
            ch == CharacterCodes.tab ||
            ch == CharacterCodes.verticalTab ||
            ch == CharacterCodes.formFeed ||
            ch == CharacterCodes.nonBreakingSpace ||
            ch == CharacterCodes.nextLine ||
            ch == CharacterCodes.ogham ||
            ch >= CharacterCodes.enQuad && ch <= CharacterCodes.zeroWidthSpace ||
            ch == CharacterCodes.narrowNoBreakSpace ||
            ch == CharacterCodes.mathematicalSpace ||
            ch == CharacterCodes.ideographicSpace ||
            ch == CharacterCodes.byteOrderMark;
    }

    public List<int> getLineStarts(SourceFile sf) {
        if (sf.LineMap is null) {
            sf.LineMap = computeLineStarts(sf.Text);
        }
        return sf.LineMap;
    }

    public int getPositionOfLineAndCharacter(int line, int character) {
        return computePositionOfLineAndCharacter(getLineStarts(sourceFile), line, character, sourceFile.Text);
    }

    public int computePositionOfLineAndCharacter(List<int> lineStarts, int line, int character, string debugText) {
        if (line < 0 || line >= lineStarts.Count) {
            Debug.Fail($"Bad line number. Line: {line}");
        }

        var res = lineStarts[line] + character;
        if (line < lineStarts.Count - 1) {
            Debug.Assert(res < lineStarts[line + 1]);
        }
        Debug.Assert(res <= debugText.Length); // Allow single character overflow for trailing newline
        return res;
    }

    public LineAndCharacter computeLineAndCharacterOfPosition(List<int> lineStarts, int position) {
        var lineNumber = computeLineOfPosition(lineStarts, position);
        return new LineAndCharacter(lineNumber, position - lineStarts[lineNumber]);
    }

    public int computeLineOfPosition(List<int> lineStarts, int position) {
        var lineNumber = binarySearch(lineStarts, position);
        if (lineNumber < 0) {
            // If the actual position was not found,
            // the binary search returns the 2's-complement of the next line start
            // e.g. if the line starts at [5, 10, 23, 80] and the position requested was 20
            // then the search will return -2.
            //
            // We want the index of the previous line start, so we subtract 1.
            // Review 2's-complement if this is confusing.
            lineNumber = ~lineNumber - 1;
            Debug.Assert(lineNumber != -1, "position cannot precede the beginning of the file");
        }
        return lineNumber;
    }

    private int binarySearch(List<int> lineStarts, int position) {
        var low = 0;
        var high = lineStarts.Count - 1;
        while (low <= high) {
            var mid = low + (high - low) / 2;
            if (position < lineStarts[mid]) {
                high = mid - 1;
            } else if (position > lineStarts[mid]) {
                low = mid + 1;
            } else {
                return lineStarts[mid];
            }
        }

        return ~low;
    }

    public LineAndCharacter getLineAndCharacterOfPosition(int position) {
        return computeLineAndCharacterOfPosition(getLineStarts(sourceFile), position);
    }

    public bool isLineBreak(CharacterCodes ch) {
        return ch == CharacterCodes.lineFeed ||
            ch == CharacterCodes.carriageReturn ||
            ch == CharacterCodes.lineSeparator ||
            ch == CharacterCodes.paragraphSeparator;
    }

    public bool HasPrecedingLineBreak() => (TokenFlags & TokenFlags.PrecedingLineBreak) != 0;

    public SyntaxKind Scan() {
        FullStartPos = pos;
        TokenFlags = TokenFlags.None;

        for (; ; ) {
            TokenStart = pos;
            if (pos >= end) {
                return AddToken(SyntaxKind.EndOfFileToken);
            }

            var ch = ReadChar();
            switch (ch) {
            case CharacterCodes.lineFeed:
            case CharacterCodes.carriageReturn:
                TokenFlags |= TokenFlags.PrecedingLineBreak;
                pos++;
                continue;
            case CharacterCodes.tab:
            case CharacterCodes.verticalTab:
            case CharacterCodes.formFeed:
            case CharacterCodes.space:
            case CharacterCodes.nonBreakingSpace:
            case CharacterCodes.ogham:
            case CharacterCodes.enQuad:
            case CharacterCodes.emQuad:
            case CharacterCodes.enSpace:
            case CharacterCodes.emSpace:
            case CharacterCodes.threePerEmSpace:
            case CharacterCodes.fourPerEmSpace:
            case CharacterCodes.sixPerEmSpace:
            case CharacterCodes.figureSpace:
            case CharacterCodes.punctuationSpace:
            case CharacterCodes.thinSpace:
            case CharacterCodes.hairSpace:
            case CharacterCodes.zeroWidthSpace:
            case CharacterCodes.narrowNoBreakSpace:
            case CharacterCodes.mathematicalSpace:
            case CharacterCodes.ideographicSpace:
            case CharacterCodes.byteOrderMark:
                pos++;
                continue;
            case CharacterCodes.ampersand:
                if (MatchChar(CharacterCodes.ampersand)) {
                    pos += 2;
                    return AddToken(SyntaxKind.AmpersandAmpersandToken);
                } else {
                    pos++;
                    return AddToken(SyntaxKind.AmpersandToken);
                }
            case CharacterCodes.bar:
                if (MatchChar(CharacterCodes.bar)) {
                    pos += 2;
                    return AddToken(SyntaxKind.BarBarToken);
                } else {
                    pos++;
                    return AddToken(SyntaxKind.BarToken);
                }
            case CharacterCodes.exclamation:
                if (MatchChar(CharacterCodes.equals)) {
                    pos += 2;
                    return AddToken(SyntaxKind.ExclamationEqualsToken);
                } else {
                    pos++;
                    return AddToken(SyntaxKind.ExclamationToken);
                }
            case CharacterCodes.doubleQuote:
                return AddToken(SyntaxKind.StringLiteral, ScanString());
            case CharacterCodes.singleQuote:
                return AddToken(SyntaxKind.CharacterLiteral, ScanCharacter());
            case CharacterCodes.comma:
                pos++;
                return AddToken(SyntaxKind.CommaToken);
            case CharacterCodes.openParen:
                pos++;
                return AddToken(SyntaxKind.OpenParenToken);
            case CharacterCodes.closeParen:
                pos++;
                return AddToken(SyntaxKind.CloseParenToken);
            case CharacterCodes.openBracket:
                pos++;
                return AddToken(SyntaxKind.OpenBracketToken);
            case CharacterCodes.closeBracket:
                pos++;
                return AddToken(SyntaxKind.CloseBracketToken);
            case CharacterCodes.openBrace:
                pos++;
                return AddToken(SyntaxKind.OpenBraceToken);
            case CharacterCodes.closeBrace:
                pos++;
                return AddToken(SyntaxKind.CloseBraceToken);
            case CharacterCodes.semicolon:
                pos++;
                return AddToken(SyntaxKind.SemicolonToken);
            case CharacterCodes.colon:
                pos++;
                return AddToken(SyntaxKind.ColonToken);
            case CharacterCodes.plus:
                pos++;
                return AddToken(SyntaxKind.PlusToken);
            case CharacterCodes.minus:
                if (MatchChar(CharacterCodes.greaterThan)) {
                    pos += 2;
                    return AddToken(SyntaxKind.MinusGreaterThanToken);
                } else {
                    pos++;
                    return AddToken(SyntaxKind.MinusToken);
                }
            case CharacterCodes.asterisk:
                pos++;
                return AddToken(SyntaxKind.AsteriskToken);
            case CharacterCodes.slash:
                if (MatchChar(CharacterCodes.slash)) {
                    SingleLineComment();
                    continue;
                } else if (MatchChar(CharacterCodes.asterisk)) {
                    MultiLineComment();
                    continue;
                } else {
                    pos++;
                    return AddToken(SyntaxKind.SlashToken);
                }
            case CharacterCodes.equals:
                if (MatchChar(CharacterCodes.equals)) {
                    pos += 2;
                    return AddToken(SyntaxKind.EqualsEqualsToken);
                } else {
                    pos++;
                    return AddToken(SyntaxKind.EqualsToken);
                }
            case CharacterCodes.lessThan:
                if (MatchChar(CharacterCodes.equals)) {
                    pos += 2;
                    return AddToken(SyntaxKind.LessThanEqualsToken);
                } else {
                    pos++;
                    return AddToken(SyntaxKind.LessThanToken);
                }
            case CharacterCodes.greaterThan:
                if (MatchChar(CharacterCodes.equals)) {
                    pos += 2;
                    return AddToken(SyntaxKind.GreaterThanEqualsToken);
                } else {
                    pos++;
                    return AddToken(SyntaxKind.GreaterThanToken);
                }
            default:
                if (char.IsLetter((char)ch)) {
                    return ScanIdentifier();
                }
                if (char.IsDigit((char)ch)) {
                    return ScanNumber();
                }

                return SyntaxKind.Unknown;
            }
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

    private SyntaxKind ScanNumber() {
        var start = pos;
        pos++;
        var result = "";

        while (char.IsDigit((char)ReadChar())) {
            pos++;
        }

        if (ReadChar() != CharacterCodes.dot) {
            result = text.Substring(start, pos - start);
            return AddToken(SyntaxKind.IntLiteral, result);
        }
        pos++;

        while (char.IsDigit((char)ReadChar())) {
            pos++;
        }

        result = text.Substring(start, pos - start);
        return AddToken(SyntaxKind.FloatLiteral, result);
    }

    private SyntaxKind ScanIdentifier() {
        var start = pos;
        pos++;

        while (char.IsLetterOrDigit((char)ReadChar())) {
            pos++;
        }

        var result = text.Substring(start, pos - start);
        if (TextToKeyword.TryGetValue(result, out var kind)) {
            return AddToken(kind);
        }

        return AddToken(SyntaxKind.Identifier, result);
    }

    private string ScanString() {
        var quote = ReadChar();
        pos++;

        var start = pos;
        var result = "";

        for (; ; ) {
            if (pos >= end) {
                result = text.Substring(start, pos - start);
                TokenFlags |= TokenFlags.Unterminated;
                throw new ParseError($"{LineColumn(pos)}: Unterminated string literal.");
            }
            var ch = ReadChar();
            if (ch == quote) {
                result = text.Substring(start, pos - start);
                pos++;
                break;
            }
            pos++;
        }

        return result;
    }

    private string ScanCharacter() {
        var quote = ReadChar();
        pos++;

        var result = "";

        var ch = ReadChar();
        if (ch != quote) {
            result = text.Substring(pos, 1);
            pos++;
        }
        pos++;

        return result;
    }

    private CharacterCodes ReadChar() {
        var ch = text[pos];
        return (CharacterCodes)ch;
    }

    private bool MatchChar(CharacterCodes c) {
        if (PeekChar() == c) {
            return true;
        } else {
            return false;
        }
    }

    private CharacterCodes PeekChar() {
        var ch = text[pos + 1];
        return (CharacterCodes)ch;
    }

    private void MultiLineComment() {
        pos += 2;
        var commentClosed = false;
        var lastLineStart = TokenStart;
        while (pos < end) {
            var ch = ReadChar();
            if (ch == CharacterCodes.asterisk && PeekChar() == CharacterCodes.slash) {
                pos += 2;
                commentClosed = true;
                break;
            }

            pos++;

            if (isLineBreak(ch)) {
                lastLineStart = pos;
                TokenFlags |= TokenFlags.PrecedingLineBreak;
            }
        }

        if (!commentClosed) {
            throw new ParseError($"{LineColumn(pos)}: Unterminated multi-line comment.");
        }
    }

    private void SingleLineComment() {
        pos += 2;
        while (pos < end) {
            if (isLineBreak(ReadChar())) {
                break;
            }
            pos++;
        }
    }
}
