namespace Gaia.Domain;

public enum SyntaxKind {
    Unknown,
    EndOfFileToken,

    // Literals
    IntLiteral,
    FloatLiteral,
    CharacterLiteral,
    StringLiteral,

    // Punctuation
    OpenBraceToken,
    CloseBraceToken,
    OpenParenToken,
    CloseParenToken,
    OpenBracketToken,
    CloseBracketToken,
    DotToken,
    SemicolonToken,
    CommaToken,
    LessThanToken,
    GreaterThanToken,
    LessThanEqualsToken,
    GreaterThanEqualsToken,
    EqualsEqualsToken,
    ExclamationEqualsToken,
    EqualsGreaterThanToken,
    MinusGreaterThanToken,
    PlusToken,
    MinusToken,
    AsteriskToken,
    SlashToken,
    PercentToken,
    PlusPlusToken,
    MinusMinusToken,
    AmpersandToken,
    BarToken,
    ExclamationToken,
    AmpersandAmpersandToken,
    BarBarToken,
    ColonToken,
    PlusEqualsToken,
    MinusEqualsToken,
    AsteriskEqualsToken,
    SlashEqualsToken,
    PercentEqualsToken,

    // Assignments
    EqualsToken,

    Identifier,

    // Keywords
    IntKeyword,
    CharKeyword,
    StringKeyword,
    TrueKeyword,
    FalseKeyword,
    FuncKeyword,
    StructKeyword,
    IfKeyword,
    ForKeyword,
    DoKeyword,
    ElseKeyword,
    BreakKeyword,
    WhileKeyword,
    ImportKeyword,
    ReturnKeyword,
    VarKeyword,
    PackageKeyword,
    VoidKeyword,
    NullKeyword,
    EnumKeyword,
    InterfaceKeyword,

    // TypeMember
    PropertySignature,
    MethodSignature,

    // Type
    ArrayType,
    IndexedAccessType,

    // Expression
    ArrayLiteralExpression,
    BinaryExpression,
    UnaryExpression,
    CallExpression,

    // Element
    PackageDeclaration,
    VariableDeclaration,
    FunctionDeclaration,
    Block,
    AssignStatement,
    ReturnStatement,
    WhileStatement,
    ImportDeclaration,
    DoStatement,
    BreakStatement,
    IfStatement,
    ElementAssignStatement,
    ExpressionStatement,
    StructDeclaration,
    InterfaceDeclaration,
    EnumDeclaration,

    // Top-level nodes
    SourceFile,
}
