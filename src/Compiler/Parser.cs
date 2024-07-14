using Gaia.AST;
using Gaia.Domain;

namespace Gaia.Compiler;

public sealed class Parser {
    private readonly Scanner scanner;
    private SyntaxKind currentToken;

    private readonly Dictionary<SyntaxKind, int> binaryOperatorPrecedence = new() {
        { SyntaxKind.AsteriskToken, 13 },
        { SyntaxKind.SlashToken, 13 },
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

    /// <summary>
    /// Match and move to the next token.
    /// </summary>
    /// <param name="kind"></param>
    private bool ParseExpected(SyntaxKind kind, bool shouldAdvance = true) {
        if (Token() == kind) {
            if (shouldAdvance) {
                NextToken();
            }
            return true;
        }

        throw new ParseError($"expected {kind}, but got {Token()}");
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
        return scanner.TokenStart;
    }

    private int GetTokenFullStart() {
        return scanner.FullStartPos;
    }

    public string LineColumn(int pos) {
        return scanner.LineColumn(pos);
    }

    private bool ParseSemicolon() {
        return TryParseSemicolon() || ParseExpected(SyntaxKind.SemicolonToken);
    }

    public Statement ParsePackage() {
        var pos = GetNodePos();
        var token = Token();
        if (token != SyntaxKind.PackageKeyword) {
            throw new ParseError($"{LineColumn(pos)} expected package, but got {token}");
        }

        // Match package statement.
        ParseExpected(SyntaxKind.PackageKeyword);
        var idPos = GetNodePos();
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        var pkgId = new Identifier(tokenValue, idPos, GetTokenFullStart());
        ParseSemicolon();
        var end = GetTokenFullStart();

        var list = ToplevelStatements();
        var pkg = new PackageDeclaration(pkgId, list, pos, end);
        return pkg;
    }

    public List<Statement> ToplevelStatements() {
        // import statements must come first.
        var list = ImportDeclarations();

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
            var pos = GetNodePos();
            var tokenValue = GetTokenValue();
            ParseExpected(SyntaxKind.StringLiteral);
            var import = new ImportDeclaration(tokenValue, pos, GetTokenFullStart());
            ParseSemicolon();
            list.Add(import);
        }
        return list;
    }

    public Statement ParseStructDeclaration() {
        ParseExpected(SyntaxKind.StructKeyword);
        var pos = GetNodePos();
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        var id = new Identifier(tokenValue, pos, GetTokenFullStart());

        ParseExpected(SyntaxKind.OpenBraceToken);
        var props = PropertyList();
        ParseExpected(SyntaxKind.CloseBraceToken);

        return new StructDeclaration(id, props);
    }

    private List<PropertySignature> PropertyList() {
        var list = new List<PropertySignature>();
        while (Token() == SyntaxKind.Identifier) {
            var pos = GetNodePos();
            var tokenValue = GetTokenValue();
            ParseExpected(SyntaxKind.Identifier);
            var id = new Identifier(tokenValue, pos, GetTokenFullStart());
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
        var pos = GetNodePos();
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        var id = new Identifier(tokenValue, pos, GetTokenFullStart());

        ParseExpected(SyntaxKind.OpenBraceToken);
        var methods = MethodList();
        ParseExpected(SyntaxKind.CloseBraceToken);

        return new InterfaceDeclaration(id, methods);
    }

    private List<MethodSignature> MethodList() {
        var list = new List<MethodSignature>();
        while (Token() == SyntaxKind.Identifier) {
            var pos = GetNodePos();
            var tokenValue = GetTokenValue();
            ParseExpected(SyntaxKind.Identifier);
            var methodName = new Identifier(tokenValue, pos, GetTokenFullStart());

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
        var pos = GetNodePos();
        ParseExpected(SyntaxKind.VarKeyword);
        var idPos = GetNodePos();
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        var id = new Identifier(tokenValue, idPos, GetTokenFullStart());

        ParseExpected(SyntaxKind.ColonToken);
        var typ = ParseType();

        Expression? x = null;
        if (Token() == SyntaxKind.EqualsToken) {
            ParseExpected(SyntaxKind.EqualsToken);
            x = ParseExpression();
        }
        var varStmt = new VariableDeclaration(id, typ, x, pos, GetTokenFullStart());
        ParseSemicolon();
        return varStmt;
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
        case SyntaxKind.BoolKeyword:
            typ = new KeywordLikeNode(token, pos, GetTokenFullStart());
            NextToken();
            break;
        default:
            throw new ParseError("no valid type");
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
            throw new ParseError("array type error");
        }
    }

    private ArrayType ParseArrayType(Expression a) {
        var pos = GetNodePos();
        if (Token() == SyntaxKind.OpenBracketToken) {
            NextToken();
            ParseExpected(SyntaxKind.CloseBracketToken);
            a = ParseArrayType(a);
        }
        return new ArrayType(a, pos, GetTokenFullStart());
    }

    private IndexedAccessType ParseIndexedAccess(Expression x, string s) {
        var pos = GetNodePos();
        if (Token() == SyntaxKind.OpenBracketToken) {
            NextToken();
            var tokenValue = GetTokenValue();
            ParseExpected(SyntaxKind.IntLiteral);
            ParseExpected(SyntaxKind.CloseBracketToken);
            x = ParseIndexedAccess(x, tokenValue);
        }
        return new IndexedAccessType(x, s, pos, GetTokenFullStart());
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
            x = new LiteralLikeNode(token, tokenValue, pos, GetTokenFullStart());
            NextToken();
            return x;
        case SyntaxKind.Identifier:
            return ParseAccessOrCall(pos, tokenValue);
        case SyntaxKind.OpenBracketToken:
            return ParseArrayLiteral();
        default:
            throw new ParseError("ParsePrimaryExpression has a bug");
        }
    }

    public Expression ParseArrayLiteral() {
        var pos = GetNodePos();
        ParseExpected(SyntaxKind.OpenBracketToken);
        var list = new List<Expression>();
        var first = ParseUnaryExpression();
        list.Add(first);

        while (Token() == SyntaxKind.CommaToken) {
            ParseExpected(SyntaxKind.CommaToken);
            if (Token() == SyntaxKind.CloseBracketToken) {
                break;
            }
            var elem = ParseUnaryExpression();
            list.Add(elem);
        }
        ParseExpected(SyntaxKind.CloseBracketToken);

        return new ArrayLiteralExpression(list, pos, GetTokenFullStart());
    }

    public Statement ParseEnumDeclaration() {
        var pos = GetNodePos();
        ParseExpected(SyntaxKind.EnumKeyword);
        var idPos = GetNodePos();
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        var id = new Identifier(tokenValue, idPos, GetTokenFullStart());

        ParseExpected(SyntaxKind.OpenBraceToken);
        var members = EnumMemberList();
        ParseExpected(SyntaxKind.CloseBraceToken);

        return new EnumDeclaration(id, members);
    }

    private List<EnumMember> EnumMemberList() {
        var list = new List<EnumMember>();

        while (Token() == SyntaxKind.Identifier) {
            var pos = GetNodePos();
            var tokenValue = GetTokenValue();
            ParseExpected(SyntaxKind.Identifier);
            var id = new Identifier(tokenValue, pos, GetTokenFullStart());

            Expression? expr = null;
            if (Token() == SyntaxKind.CommaToken) {
                ParseExpected(SyntaxKind.CommaToken);
            } else if (Token() == SyntaxKind.EqualsToken) {
                ParseExpected(SyntaxKind.EqualsToken);
                expr = ParseUnaryExpression();
                ParseExpected(SyntaxKind.CommaToken);
            }

            var member = new EnumMember(id, expr);
            list.Add(member);
        }
        return list;
    }

    private Expression ParseAccessOrCall(int pos, string tokenValue) {
        var id = new Identifier(tokenValue, pos, GetTokenFullStart());
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
        var pos = GetNodePos();
        var tokenValue = GetTokenValue();
        ParseExpected(SyntaxKind.Identifier);
        var funcId = new Identifier(tokenValue, pos, GetTokenFullStart());

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
        var id = new Identifier(tokenValue, pos, GetTokenFullStart());

        ParseExpected(SyntaxKind.ColonToken);
        var typ = ParseType();
        var para = new Parameter(id, typ, pos, GetTokenFullStart());
        args.Add(para);
    }

    private Expression ParseReturnType() {
        if (Token() != SyntaxKind.MinusGreaterThanToken) {
            return new KeywordLikeNode(SyntaxKind.VoidKeyword, GetNodePos(), GetTokenFullStart());
        }

        ParseExpected(SyntaxKind.MinusGreaterThanToken);
        var typ = ParseType();
        return typ;
    }

    public Block ParseBlock() {
        var pos = GetNodePos();
        ParseExpected(SyntaxKind.OpenBraceToken);
        var statements = StatementList();
        ParseExpected(SyntaxKind.CloseBraceToken);
        return new Block(statements, pos, GetTokenFullStart());
    }

    private Expression ParseExpression(int precedence = 0) {
        var lhs = ParseUnaryExpression();

        while (precedence < GetPrecedence(Token())) {
            lhs = ParseBinaryExpression(lhs);
        }

        return lhs;
    }

    private Expression ParseBinaryExpression(Expression lhs) {
        var pos = GetNodePos();
        var token = Token();
        var precedence = GetPrecedence(token);
        NextToken();
        var rhs = ParseExpression(precedence);
        return new BinaryExpression(token, lhs, rhs, pos, GetTokenFullStart());
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
            throw new ParseError("unknown statement");
        }
    }

    private Statement? ParseAssignOrCall() {
        var pos = GetNodePos();
        var tokenValue = GetTokenValue();
        var id = new Identifier(tokenValue, pos, GetTokenFullStart());
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
        var pos = GetNodePos();
        if (Token() == SyntaxKind.EqualsToken) {
            NextToken();
            var x = ParseExpression();
            stmt = new AssignStatement(id, x, pos, GetTokenFullStart());
        } else {
            // For array type
            var x = ParseAccess(id);
            ParseExpected(SyntaxKind.EqualsToken);
            stmt = new ElementAssignStatement(x, ParseExpression(), pos, GetTokenFullStart());
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
