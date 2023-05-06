using Gaia.AST;
using Gaia.Domain;

namespace Gaia.Compiler;

public sealed class Parser {
    private readonly Scanner scanner;
    private SyntaxKind currentToken;

    private readonly Dictionary<SyntaxKind, int> binaryOperatorPrecedence = new() {
        { SyntaxKind.AsteriskToken, 13 },
        {SyntaxKind.SlashToken, 13 },
        { SyntaxKind.PercentToken, 13 },

        { SyntaxKind.PlusToken, 12 },
        { SyntaxKind.MinusToken, 12 },

        { SyntaxKind.LessThanToken, 10 },
        { SyntaxKind.LessThanEqualsToken, 10 },
        { SyntaxKind.GreaterThanToken, 10 },
        { SyntaxKind.GreaterThanEqualsToken, 10 },

        { SyntaxKind.EqualsEqualsToken, 9 },
        { SyntaxKind.ExclamationEqualsToken, 9 },

        { SyntaxKind.AmpersandToken, 8 },

        { SyntaxKind.BarToken, 6 },

        { SyntaxKind.AmpersandAmpersandToken, 5 },

        { SyntaxKind.BarBarToken, 4 },
    };

    private readonly HashSet<SyntaxKind> unaryOperators = new() {
        SyntaxKind.ExclamationToken, SyntaxKind.MinusToken
      };

    public Parser(Scanner scanner) {
        this.scanner = scanner;
        currentToken = this.scanner.Scan();
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
            var s = Token();
            Error($"expected package, but got {s}");
        }

        // Match package statement.
        ParseExpected(SyntaxKind.PackageKeyword);
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        var pkgId = new Identifier(tokenValue);
        ParseSemicolon();

        var list = ToplevelStatements();
        var pkg = new PackageDeclaration(pkgId, list);
        return pkg;
    }

    public List<Statement> ToplevelStatements() {
        var list = new List<Statement>();
        // import statements must come first.
        var imports = ImportDeclarations();
        list.AddRange(imports);

        while (Token() != SyntaxKind.EndOfFileToken) {
            switch (Token()) {
            case SyntaxKind.VarKeyword:
                var varDecl = ParseVariableDeclaration();
                list.Add(varDecl);
                break;
            case SyntaxKind.FuncKeyword:
                var funcDecl = ParseFunctionDeclaration();
                list.Add(funcDecl);
                break;
            case SyntaxKind.StructKeyword:
                var structDecl = ParseStructDeclaration();
                list.Add(structDecl);
                break;
            case SyntaxKind.InterfaceKeyword:
                var interfaceDecl = ParseInterfaceDeclaration();
                list.Add(interfaceDecl);
                break;
            case SyntaxKind.EnumKeyword:
                var enumDecl = ParseEnumDeclaration();
                list.Add(enumDecl);
                break;
            default:
                break;
            }
        }

        return list;
    }

    public List<Statement> ImportDeclarations() {
        var list = new List<Statement>();
        while (Token() == SyntaxKind.ImportKeyword) {
            NextToken();
            var tokenValue = GetTokenValue();
            ParseExpected(SyntaxKind.StringLiteral);
            ParseSemicolon();
            var i = new ImportDeclaration(tokenValue);
            list.Add(i);
        }
        return list;
    }


    public Statement ParseStructDeclaration() {
        ParseExpected(SyntaxKind.StructKeyword);
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        var id = new Identifier(tokenValue);

        ParseExpected(SyntaxKind.OpenBraceToken);
        var props = PropertyList();
        ParseExpected(SyntaxKind.CloseBraceToken);

        return new StructDeclaration(id, props);
    }

    private List<PropertySignature> PropertyList() {
        var list = new List<PropertySignature>();
        while (Token() == SyntaxKind.Identifier) {
            var tokenValue = GetTokenValue();
            ParseExpected(SyntaxKind.Identifier);
            var id = new Identifier(tokenValue);
            ParseExpected(SyntaxKind.ColonToken);
            var typ = ParseType();
            ParseSemicolon();
            var prop = new PropertySignature(id, typ);
            list.Add(prop);
        }
        return list;
    }

    public Statement ParseInterfaceDeclaration() {
        ParseExpected(SyntaxKind.InterfaceKeyword);
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        var id = new Identifier(tokenValue);

        ParseExpected(SyntaxKind.OpenBraceToken);
        var methods = MethodList();
        ParseExpected(SyntaxKind.CloseBraceToken);

        return new InterfaceDeclaration(id, methods);
    }

    private List<MethodSignature> MethodList() {
        var list = new List<MethodSignature>();
        while (Token() == SyntaxKind.Identifier) {
            var tokenValue = GetTokenValue();
            ParseExpected(SyntaxKind.Identifier);
            var methodName = new Identifier(tokenValue);

            ParseExpected(SyntaxKind.OpenParenToken);
            var parameters = ParameterList();
            ParseExpected(SyntaxKind.CloseParenToken);
            var returnType = ParseReturnType();
            ParseSemicolon();
            var method = new MethodSignature(methodName, parameters, returnType);
            list.Add(method);
        }
        return list;
    }

    public Statement ParseVariableDeclaration() {
        ParseExpected(SyntaxKind.VarKeyword);
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        ParseExpected(SyntaxKind.ColonToken);
        var typ = ParseType();

        Expression? s = null;
        if (Token() is not SyntaxKind.EqualsToken) {
            ParseSemicolon();
        } else {
            ParseExpected(SyntaxKind.EqualsToken);
            s = ParseExpression();
            ParseSemicolon();
        }
        var id = new Identifier(tokenValue);
        var VarStmt = new VariableDeclaration(id, typ, s);
        return VarStmt;
    }

    // Get a type for the variable.
    private Expression ParseType() {
        Expression typ;
        var pos = GetNodePos();
        var token = Token();
        switch (token) {
        case SyntaxKind.IntKeyword:
        case SyntaxKind.CharKeyword:
        case SyntaxKind.StringKeyword:
        case SyntaxKind.VoidKeyword:
            typ = new KeywordLikeNode(token, pos, GetTokenEnd());
            NextToken();
            break;
        default:
            Error("no valid type");
            return null;
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
    /// <param name="typ"></param>
    /// <returns></returns>
    private Expression ParseArray(Expression typ) {
        ParseExpected(SyntaxKind.OpenBracketToken);
        var token = Token();
        var tokenValue = GetTokenValue();
        switch (token) {
        case SyntaxKind.IntLiteral:
            NextToken();
            ParseExpected(SyntaxKind.CloseBracketToken);
            return ParseIndexedAccess(typ, tokenValue);
        case SyntaxKind.CloseBracketToken:
            NextToken();
            return ParseArrayType(typ);
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
        var token = Token();
        if (unaryOperators.Contains(token)) {
            NextToken();
            var operand = ParseUnaryExpression();
            return new UnaryExpression(token, operand);
        } else {
            return ParsePrimaryExpression();
        }
    }

    private int GetPrecedence(SyntaxKind token) {
        if (binaryOperatorPrecedence.TryGetValue(token, out var p)) {
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
        case SyntaxKind.CharacterLiteral:
        case SyntaxKind.TrueKeyword:
        case SyntaxKind.FalseKeyword:
            x = new LiteralLikeNode(token, tokenValue, pos, GetTokenEnd());
            NextToken();
            return x;
        case SyntaxKind.Identifier:
            return ParseAccessOrCall(tokenValue);
        case SyntaxKind.OpenBracketToken:
            return ParseArrayLiteral();
        default:
            Error("Primary has a bug");
            return null;
        }
    }

    public Expression ParseArrayLiteral() {
        var pos = GetNodePos();
        ParseExpected(SyntaxKind.OpenBracketToken);
        var list = new List<Expression>();
        var first = ParseUnaryExpression();
        list.Add(first);

        while (Token() is SyntaxKind.CommaToken) {
            ParseExpected(SyntaxKind.CommaToken);
            if (Token() is SyntaxKind.CloseBracketToken) {
                break;
            }
            var elem = ParseUnaryExpression();
            list.Add(elem);
        }
        ParseExpected(SyntaxKind.CloseBracketToken);

        return new ArrayLiteralExpression(list, pos, GetTokenEnd());
    }

    public Statement ParseEnumDeclaration() {
        var pos = GetNodePos();
        ParseExpected(SyntaxKind.EnumKeyword);
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        var id = new Identifier(tokenValue);

        ParseExpected(SyntaxKind.OpenBraceToken);
        var members = EnumMemberList();
        ParseExpected(SyntaxKind.CloseBraceToken);

        return new EnumDeclaration(id, members);
    }

    private List<EnumMember> EnumMemberList() {
        var list = new List<EnumMember>();

        while (Token() == SyntaxKind.Identifier) {
            var tokenValue = GetTokenValue();
            ParseExpected(SyntaxKind.Identifier);
            var id = new Identifier(tokenValue);

            Expression? expr = null;
            if (Token() is SyntaxKind.CommaToken) {
                ParseExpected(SyntaxKind.CommaToken);
            } else if (Token() is SyntaxKind.EqualsToken) {
                ParseExpected(SyntaxKind.EqualsToken);
                expr = ParseUnaryExpression();
                ParseExpected(SyntaxKind.CommaToken);
            }

            var member = new EnumMember(id, expr);
            list.Add(member);
        }
        return list;
    }

    private Expression ParseAccessOrCall(string tokenValue) {
        var id = new Identifier(tokenValue);
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

    public FunctionDeclaration ParseFunctionDeclaration() {
        ParseExpected(SyntaxKind.FuncKeyword);
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        var funcId = new Identifier(tokenValue);

        ParseExpected(SyntaxKind.OpenParenToken);
        var parameters = ParameterList();
        ParseExpected(SyntaxKind.CloseParenToken);

        var returnType = ParseReturnType();
        var body = ParseBlock();
        return new FunctionDeclaration(funcId, parameters, returnType, body);
    }

    // Formal parameters
    private List<Parameter> ParameterList() {
        var parameters = new List<Parameter>();
        if (Token() == SyntaxKind.CloseParenToken) {
            return parameters;
        }

        ParseParameter(parameters);
        ParameterRest(parameters);
        return parameters;
    }

    private void ParameterRest(List<Parameter> parameters) {
        if (Token() != SyntaxKind.CommaToken) {
            return;
        }

        ParseExpected(SyntaxKind.CommaToken);
        ParseParameter(parameters);
        // Find the rest parameters.
        ParameterRest(parameters);
    }

    private void ParseParameter(List<Parameter> args) {
        var pos = GetNodePos();
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        ParseExpected(SyntaxKind.ColonToken);
        var typ = ParseType();
        var id = new Identifier(tokenValue);
        // TODO: handle func scope
        // top?.Add(id.Name, id);
        var para = new Parameter(id, typ, pos, GetTokenEnd());
        args.Add(para);
    }

    private Expression ParseReturnType() {
        if (Token() != SyntaxKind.MinusGreaterThanToken) {
            return new KeywordLikeNode(SyntaxKind.VoidKeyword, GetNodePos(), GetTokenEnd());
        }

        ParseExpected(SyntaxKind.MinusGreaterThanToken);
        var typ = ParseType();
        return typ;
    }

    public Block ParseBlock() {
        ParseExpected(SyntaxKind.OpenBraceToken);
        var statements = StatementList();
        ParseExpected(SyntaxKind.CloseBraceToken);
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
            return ParseVariableDeclaration();
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
        var id = new Identifier(tokenValue);
        ParseExpected(SyntaxKind.Identifier);

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
            var s = ParseExpression();
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
