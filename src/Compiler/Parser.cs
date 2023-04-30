using Gaia.AST;
using Gaia.Domain;
using static Gaia.Domain.SyntaxKindText;

namespace Gaia.Compiler;

public sealed class Parser {
    private readonly Scanner scanner;
    private SyntaxKind currentToken;
    private Env? top = null;

    private readonly Dictionary<string, int> binaryOperatorPrecedence = new() {
        { "*", 13 },
        { "/", 13 },
        { "%", 13 },

        { "+", 12 },
        { "-", 12 },

        { "<", 10 },
        { "<=", 10 },
        { ">", 10 },
        { ">=", 10 },

        { "==", 9 },
        { "!=", 9 },

        { "&", 8 },

        { "|", 6 },

        { "&&", 5 },

        { "||", 4 },
    };

    private readonly HashSet<string> unaryOperators = new HashSet<string>() { "-", "!" };

    public Parser(Scanner scanner) {
        this.scanner = scanner;
        currentToken = scanner.Scan();
    }

    public Node Parse() {
        return ParsePackage();
    }

    /// <summary>
    /// Return the next token.
    /// </summary>
    /// <returns></returns>
    private SyntaxKind NextToken() {
        return currentToken = scanner.Scan();
    }

    /// <summary>
    /// Use this method instead of using to currentToken directly.
    /// </summary>
    /// <returns></returns>
    private SyntaxKind Token() {
        return currentToken;
    }

    private string GetTokenValue() {
        return scanner.TokenValue;
    }

    public static void Error(string s) {
        throw new Exception($"Near line {Scanner.Line} column {Scanner.Column}: {s}.");
    }

    /// <summary>
    /// Match and move to the next token.
    /// </summary>
    /// <param name="t"></param>
    private bool ParseExpected(SyntaxKind t, bool shouldAdvance = true) {
        if (Token() == t) {
            if (shouldAdvance) {
                NextToken();
            }
            return true;
        }

        Error($"expected {t}, but got {Token()}");
        return false;
    }

    private bool CanParseSemicolon() {
        // If there's a real semicolon, then we can always parse it out.
        if (Token() == SyntaxKind.SemicolonToken) {
            return true;
        }

        // We can parse out an optional semicolon in ASI cases in the following cases.
        return Token() == SyntaxKind.CloseBraceToken || Token() == SyntaxKind.EndOfFileToken || scanner.HasPrecedingLineBreak();
    }

    private bool TryParseSemicolon() {
        if (!CanParseSemicolon()) {
            return false;
        }

        if (Token() == SyntaxKind.SemicolonToken) {
            // consume the semicolon if it was explicitly provided.
            NextToken();
        }

        return true;
    }

    private int GetNodePos() {
        return scanner.FullStartPos;
    }

    private int GetTokenEnd() {
        return scanner.TokenEnd;
    }

    private bool ParseSemicolon() {
        return TryParseSemicolon() || ParseExpected(SyntaxKind.SemicolonToken);
    }

    public Statement ParsePackage() {
        if (Token() != SyntaxKind.PackageKeyword) {
            Error($"expected package, but got {currentToken}");
        }

        var savedEnv = top;
        top = new Env(top);

        // Match package statement.
        ParseExpected(SyntaxKind.PackageKeyword);
        var tok = Token();
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        var id = new Identifier(tokenValue);
        top?.Add(tokenValue, id);
        ParseSemicolon();

        var list = TopLevelStatements();
        var p = new PackageDeclaration(id.Name, list);
        top = savedEnv;
        return p;
    }

    public List<Statement> TopLevelStatements() {
        var list = new List<Statement>();

        // import statements must come first.
        var imports = ImportDeclarations();
        list.AddRange(imports);

        while (Token() != SyntaxKind.EndOfFileToken) {
            switch (Token()) {
            case SyntaxKind.VarKeyword:
                var varStmt = ParseVar();
                list.Add(varStmt);
                break;
            case SyntaxKind.FuncKeyword:
                var funcStmt = ParseFuncStatement();
                list.Add(funcStmt);
                break;
            case SyntaxKind.StructKeyword:
                var structStmt = ParseStruct();
                list.Add(structStmt);
                break;
            default:
                break;
            }
        }

        return list;
    }

    // TODO:
    public Statement ParseStruct() {
        ParseExpected(SyntaxKind.StructKeyword);
        var tok = currentToken;
        ParseExpected(SyntaxKind.Identifier);
        var tokenValue = GetTokenValue();
        var id = new Identifier(tokenValue);
        top?.Add(tokenValue, id);

        ParseExpected(SyntaxKind.OpenBraceToken);
        var props = PropertyList();
        ParseExpected(SyntaxKind.CloseBraceToken);

        return new StructDeclaration(id, props);
    }

    private List<PropertySignature> PropertyList() {
        var list = new List<PropertySignature>();
        while (Token() == SyntaxKind.Identifier) {
            var tok = currentToken;
            var tokenValue = GetTokenValue();
            ParseExpected(SyntaxKind.Identifier);
            var id = new Identifier(tokenValue);
            top?.Add(tokenValue, id);
            // TODO:
            // list.Add(id);
        }
        return list;
    }

    public List<Statement> ImportDeclarations() {
        var list = new List<Statement>();
        while (Token() == SyntaxKind.ImportKeyword) {
            NextToken();
            var tok = currentToken;
            var tokenValue = GetTokenValue();
            ParseExpected(SyntaxKind.StringLiteral);
            ParseSemicolon();
            var i = new ImportDeclaration(tokenValue);
            list.Add(i);
        }
        return list;
    }

    public Statement ParseVar() {
        ParseExpected(SyntaxKind.VarKeyword);
        var tok = currentToken;
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        ParseExpected(SyntaxKind.ColonToken);
        var typ = GetTypeInfo();

        Expression? s = null;
        if (Token() != SyntaxKind.EqualsToken) {
            ParseSemicolon();
        } else {
            ParseExpected(SyntaxKind.EqualsToken);
            s = ParseExpression();
            ParseSemicolon();
        }
        var id = new Identifier(tokenValue);
        top?.Add(tokenValue, id);
        var VarStmt = new VariableDeclaration(id, typ, s);
        return VarStmt;
    }

    // Get a type for the variable.
    private Expression GetTypeInfo() {
        Expression typ;
        var token = Token();
        var pos = GetNodePos();
        switch (token) {
        case SyntaxKind.IntKeyword:
        case SyntaxKind.StringKeyword:
            typ = new BaseTokenNode(token, pos, GetTokenEnd());
            NextToken();
            break;
        default:
            Error("no valid type");
            throw new Exception();
        }

        if (Token() != SyntaxKind.OpenBracketToken) {
            return typ;
        } else {
            return ParseArray(typ);
        }
    }

    /// <summary>
    /// Array types.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    private Expression ParseArray(Expression p) {
        ParseExpected(SyntaxKind.OpenBracketToken);
        var tok = currentToken;
        var tokenValue = GetTokenValue();
        switch (tok) {
        case SyntaxKind.IntLiteral:
            NextToken();
            ParseExpected(SyntaxKind.CloseBracketToken);
            return ParseIndexedAccess(p, tokenValue);
        case SyntaxKind.CloseBracketToken:
            NextToken();
            return ParseArrayType(p);
        default:
            Error("array type error");
            return null;
        }
    }

    private ArrayType ParseArrayType(Expression a) {
        var pos = GetNodePos();
        if (Token() == SyntaxKind.OpenBracketToken) {
            NextToken();
            ParseExpected(SyntaxKind.CloseBracketToken);
            a = ParseArrayType(a);
        }
        return new ArrayType(a, pos, GetTokenEnd());
    }

    private IndexedAccessType ParseIndexedAccess(Expression p, string s) {
        var pos = GetNodePos();
        if (Token() == SyntaxKind.OpenBracketToken) {
            NextToken();
            var tokenValue = GetTokenValue();
            ParseExpected(SyntaxKind.IntLiteral);
            ParseExpected(SyntaxKind.CloseBracketToken);
            p = ParseIndexedAccess(p, tokenValue);
        }
        return new IndexedAccessType(p, s, pos, GetTokenEnd());
    }

    private Expression ParseUnaryExpression() {
        var tokenValue = GetTokenValue();
        if (unaryOperators.Contains(tokenValue)) {
            NextToken();
            var op = Token();
            var operand = ParseUnaryExpression();
            return new UnaryExpression(op, operand);
        } else {
            return ParsePrimaryExpression();
        }
    }

    private int GetPrecedence(SyntaxKind token) {
        if (binaryOperatorPrecedence.TryGetValue(TokenStrings[token], out var p)) {
            return p;
        }

        return 0;
    }

    /// <summary>
    /// Factor.
    /// </summary>
    /// <returns></returns>
    private Expression ParsePrimaryExpression() {
        Expression x;
        var token = Token();
        var pos = GetNodePos();
        var tokenValue = GetTokenValue();
        switch (token) {
        case SyntaxKind.OpenParenToken:
            // Ignore unnecessary parens.
            NextToken();
            x = ParseExpression();
            ParseExpected(SyntaxKind.CloseParenToken);
            return x;
        case SyntaxKind.IntLiteral:
        case SyntaxKind.FloatLiteral:
        case SyntaxKind.StringLiteral:
        case SyntaxKind.TrueKeyword:
        case SyntaxKind.FalseKeyword:
            x = new LiteralLikeNode(token, pos, GetTokenEnd(), tokenValue);
            NextToken();
            return x;
        case SyntaxKind.Identifier:
            return ParseAccessOrCall(tokenValue);
        case SyntaxKind.OpenBracketToken:
            return ParseArrayLiteral(pos);
        default:
            Error("Primary has a bug");
            // unreachable
            return null;
        }
    }

    public Expression ParseArrayLiteral(int pos) {
        ParseExpected(SyntaxKind.OpenBracketToken);
        var list = new List<Expression>();
        var first = ParseUnaryExpression();
        list.Add(first);
        while (Token() == SyntaxKind.CommaToken) {
            ParseExpected(SyntaxKind.CommaToken);
            var elem = ParseUnaryExpression();
            list.Add(elem);
        }
        ParseExpected(SyntaxKind.CloseBracketToken);
        return new ArrayLiteralExpression(list, pos);
    }

    private Expression ParseAccessOrCall(string tokenValue) {
        var id = top?.Get(tokenValue);
        if (id is null) {
            // Error($"{look} undeclared");
            // TODO: Handle undefined functions
            id = new Identifier(tokenValue);
        }
        NextToken();

        if (Token() == SyntaxKind.OpenParenToken) {
            return ParseCall(id);
        } else if (Token() == SyntaxKind.OpenBracketToken) {
            return ParseAccess(id);

        } else {
            return id;
        }
    }

    // Actual arguments
    private List<Expression> ArgumentList() {
        var args = new List<Expression>();
        if (Token() == SyntaxKind.CloseParenToken) {
            return args;
        }

        var first = ParseExpression();
        args.Add(first);

        ArgumentRest(args);
        return args;
    }

    private void ArgumentRest(List<Expression> args) {
        if (Token() != SyntaxKind.CommaToken) {
            return;
        }

        ParseExpected(SyntaxKind.CommaToken);
        var rest = ParseExpression();
        args.Add(rest);

        // Find the rest.
        ArgumentRest(args);
    }

    private CallExpression ParseCall(Expression expr) {
        ParseExpected(SyntaxKind.OpenParenToken);
        var args = ArgumentList();
        ParseExpected(SyntaxKind.CloseParenToken);
        var call = new CallExpression(expr, args);
        if (Token() == SyntaxKind.OpenParenToken) {
            call = ParseCall(call);
        }

        return call;
    }

    public FunctionDeclaration ParseFuncStatement() {
        var savedEnv = top;
        top = new Env(top);

        ParseExpected(SyntaxKind.FuncKeyword);
        var tokenValue = GetTokenValue();

        ParseExpected(SyntaxKind.Identifier);

        var funcId = new Identifier(tokenValue);
        top?.Add(funcId.Name, funcId);

        ParseExpected(SyntaxKind.OpenParenToken);
        var args = ParameterList();
        ParseExpected(SyntaxKind.CloseParenToken);

        var returnType = ReturnType();
        var b = ParseBlock();
        return new FunctionDeclaration(funcId, args, returnType, b);
    }

    // Formal parameters
    private List<Identifier> ParameterList() {
        var args = new List<Identifier>();
        if (Token() == SyntaxKind.CloseParenToken) {
            return args;
        }

        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        ParseExpected(SyntaxKind.ColonToken);
        var p = GetTypeInfo();
        var id = new Identifier(tokenValue);
        top?.Add(id.Name, id);
        args.Add(id);

        ParameterRest(args);
        return args;
    }

    private void ParameterRest(List<Identifier> args) {
        if (Token() != SyntaxKind.CommaToken) {
            return;
        }

        ParseExpected(SyntaxKind.CommaToken);
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        ParseExpected(SyntaxKind.ColonToken);
        var p = GetTypeInfo();
        var id = new Identifier(tokenValue);
        top?.Add(id.Name, id);
        args.Add(id);

        // Find the rest parameters.
        ParameterRest(args);
    }

    private Expression ReturnType() {
        if (Token() != SyntaxKind.MinusGreaterThanToken) {
            return new LiteralLikeNode(SyntaxKind.VoidKeyword, GetNodePos(), GetTokenEnd(), "void");
        }

        ParseExpected(SyntaxKind.MinusGreaterThanToken);
        var p = GetTypeInfo();
        return p;
    }

    public Block ParseBlock() {
        ParseExpected(SyntaxKind.OpenBraceToken);

        var savedEnv = top;
        top = new Env(top);

        var statements = StatementList();

        ParseExpected(SyntaxKind.CloseBraceToken);
        top = savedEnv;
        return new Block(statements);
    }

    private Expression ParseExpression(int precedence = 0) {
        var lhs = ParseUnaryExpression();

        while (precedence < GetPrecedence(Token())) {
            lhs = ParseBinaryExpression(lhs);
        }

        return lhs;
    }

    private Expression ParseBinaryExpression(Expression lhs) {
        var token = Token();
        var precedence = GetPrecedence(token);
        NextToken();
        var rhs = ParseExpression(precedence);
        return new BinaryExpression(token, lhs, rhs);
    }

    private List<Statement> StatementList() {
        var list = new List<Statement>();
        while (Token() != SyntaxKind.CloseBraceToken && Token() != SyntaxKind.FuncKeyword && Token() != SyntaxKind.EndOfFileToken) {
            var stmt = ParseStatement();
            if (stmt is not null) {
                list.Add(stmt);
            }
        }
        return list;
    }

    private Statement? ParseStatement() {
        switch (Token()) {
        case SyntaxKind.SemicolonToken:
            NextToken();
            return null;
        case SyntaxKind.VarKeyword:
            return ParseVar();
        case SyntaxKind.WhileKeyword:
            NextToken();
            var whileExpr = ParseExpression();
            var whileBlock = ParseBlock();
            var whileStmt = new WhileStatement(whileExpr, whileBlock);
            return whileStmt;
        case SyntaxKind.IfKeyword:
            return ParseIfStatement();
        case SyntaxKind.BreakKeyword:
            NextToken();
            ParseSemicolon();
            return new BreakStatement();
        case SyntaxKind.DoKeyword:
            NextToken();
            var doBlock = ParseBlock();
            ParseExpected(SyntaxKind.WhileKeyword);
            var doExpr = ParseExpression();
            ParseSemicolon();
            return new DoStatement(doBlock, doExpr);
        case SyntaxKind.OpenBraceToken:
            return ParseBlock();
        case SyntaxKind.ReturnKeyword:
            return ParseReturn();
        case SyntaxKind.Identifier:
            return ParseAssignOrCall();
        default:
            Error("unknown statement");
            return null;
        }
    }

    private Statement? ParseAssignOrCall() {
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        var id = top?.Get(tokenValue);
        if (id is null) {
            // Error($"{tok} undeclared");
            id = new Identifier(tokenValue);
        }

        if (Token() == SyntaxKind.OpenParenToken) {
            var call = ParseCall(id);
            return new ExpressionStatement(call);
        } else {
            return ParseAssign(id);
        }
    }

    private Statement ParseIfStatement() {
        ParseExpected(SyntaxKind.IfKeyword);
        var expr = ParseExpression();
        var thenBlock = ParseBlock();
        if (Token() != SyntaxKind.ElseKeyword) {
            return new IfStatement(expr, thenBlock);
        }
        ParseExpected(SyntaxKind.ElseKeyword);

        if (Token() != SyntaxKind.IfKeyword) {
            var elseBlock = ParseBlock();
            return new IfStatement(expr, thenBlock, elseBlock);
        }

        var ifStmt = ParseIfStatement();
        return new IfStatement(expr, thenBlock, ifStmt);
    }

    public Statement ParseReturn() {
        ParseExpected(SyntaxKind.ReturnKeyword);

        if (CanParseSemicolon()) {
            return new ReturnStatement();
        } else {
            var s = ParseUnaryExpression();
            ParseSemicolon();
            return new ReturnStatement(s);
        }
    }

    private Statement? ParseAssign(Identifier id) {
        Statement? stmt = null;
        if (Token() == SyntaxKind.EqualsToken) {
            NextToken();
            stmt = new AssignStatement(id, ParseExpression());
        } else {
            // For array type
            var x = ParseAccess(id);
            ParseExpected(SyntaxKind.EqualsToken);
            stmt = new ElementAssignStatement(x, ParseExpression());
        }

        ParseSemicolon();
        return stmt;
    }

    public ElementAccessExpression ParseAccess(Expression expr) {
        ParseExpected(SyntaxKind.OpenBracketToken);
        var index = ParseExpression();
        ParseExpected(SyntaxKind.CloseBracketToken);
        var access = new ElementAccessExpression(expr, index);
        if (Token() == SyntaxKind.OpenBracketToken) {
            access = ParseAccess(access);
        }

        return access;
    }
}
