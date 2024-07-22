namespace Gaia.Domain;

public static class SyntaxKindText {
    public static readonly Dictionary<string, SyntaxKind> TextToKeyword = new() {
        {"package", SyntaxKind.PackageKeyword},
        {"var", SyntaxKind.VarKeyword},
        {"int", SyntaxKind.IntKeyword},
        {"true", SyntaxKind.TrueKeyword},
        {"false", SyntaxKind.FalseKeyword},
        {"func", SyntaxKind.FuncKeyword},
        {"return", SyntaxKind.ReturnKeyword},
        {"import", SyntaxKind.ImportKeyword},
        {"if", SyntaxKind.IfKeyword},
        {"else", SyntaxKind.ElseKeyword},
        {"while", SyntaxKind.WhileKeyword},
        {"do", SyntaxKind.DoKeyword},
        {"for", SyntaxKind.ForKeyword},
        {"break", SyntaxKind.BreakKeyword},
        {"char", SyntaxKind.CharKeyword},
        {"string", SyntaxKind.StringKeyword},
        {"struct", SyntaxKind.StructKeyword},
        {"enum", SyntaxKind.EnumKeyword},
        {"interface", SyntaxKind.InterfaceKeyword},
        {"void", SyntaxKind.VoidKeyword},
        {"bool", SyntaxKind.BoolKeyword},
        {"null", SyntaxKind.NullKeyword},
        {"float", SyntaxKind.FloatKeyword},
    };

    private static readonly Dictionary<string, SyntaxKind> TextToToken = TextToKeyword.Union(new Dictionary<string, SyntaxKind>() {
        {"{", SyntaxKind.OpenBraceToken},
        {"}", SyntaxKind.CloseBraceToken},
        {"(", SyntaxKind.OpenParenToken},
        {")", SyntaxKind.CloseParenToken},
        {"[", SyntaxKind.OpenBracketToken},
        {"]", SyntaxKind.CloseBracketToken},
        {".", SyntaxKind.DotToken},
        {";", SyntaxKind.SemicolonToken},
        {",", SyntaxKind.CommaToken},
        {"<", SyntaxKind.LessThanToken},
        {">", SyntaxKind.GreaterThanToken},
        {"<=", SyntaxKind.LessThanEqualsToken},
        {">=", SyntaxKind.GreaterThanEqualsToken},
        {"==", SyntaxKind.EqualsEqualsToken},
        {"!=", SyntaxKind.ExclamationEqualsToken},
        {"+", SyntaxKind.PlusToken},
        {"-", SyntaxKind.MinusToken},
        {"*", SyntaxKind.AsteriskToken},
        {"/", SyntaxKind.SlashToken},
        {"%", SyntaxKind.PercentToken},
        {"++", SyntaxKind.PlusPlusToken},
        {"--", SyntaxKind.MinusMinusToken},
        {"&", SyntaxKind.AmpersandToken},
        {"|", SyntaxKind.BarToken},
        {"!", SyntaxKind.ExclamationToken},
        {"&&", SyntaxKind.AmpersandAmpersandToken},
        {"||", SyntaxKind.BarBarToken},
        {":", SyntaxKind.ColonToken},
        {"=", SyntaxKind.EqualsToken},
        {"+=", SyntaxKind.PlusEqualsToken},
        {"-=", SyntaxKind.MinusEqualsToken},
        {"*=", SyntaxKind.AsteriskEqualsToken},
        {"/=", SyntaxKind.SlashEqualsToken},
        {"%=", SyntaxKind.PercentEqualsToken},
    }).ToDictionary(x => x.Key, x => x.Value);

    private static Dictionary<SyntaxKind, string> MakeReverseMap(Dictionary<string, SyntaxKind> map) => map.ToDictionary(x => x.Value, x => x.Key);

    public static Dictionary<SyntaxKind, string> TokenToText = MakeReverseMap(TextToToken);

    public static string KindToText(SyntaxKind sk) {
        if (TokenToText.ContainsKey(sk)) {
            return TokenToText[sk];
        } else {
            return sk.ToString();
        }
    }
}
