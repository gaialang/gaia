using Gaia.AST;

namespace Gaia.Compiler;

public sealed class Parser {
    private readonly Scanner scanner;
    private Token currentToken;
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
        currentToken = this.scanner.Scan();
    }

    public Node Parse() {
        return ParsePackage();
    }

    /// <summary>
    /// Return the next token.
    /// </summary>
    /// <returns></returns>
    private Token NextToken() {
        return currentToken = scanner.Scan();
    }

    private Token GetToken() {
        return currentToken;
    }

    public static void Error(string s) {
        throw new Exception($"Near line {Scanner.Line} column {Scanner.Column}: {s}.");
    }

    /// <summary>
    /// Match and move to the next token.
    /// </summary>
    /// <param name="t"></param>
    private bool ParseExpected(TokenType t, bool shouldAdvance = true) {
        if (currentToken.Type == t) {
            if (shouldAdvance) {
                NextToken();
            }
            return true;
        }

        Error($"expected {t}, but got {currentToken}");
        return false;
    }

    private bool CanParseSemicolon() {
        var kind = currentToken.Type;
        // If there's a real semicolon, then we can always parse it out.
        if (kind == TokenType.Semicolon) {
            return true;
        }

        // We can parse out an optional semicolon in ASI cases in the following cases.
        return kind == TokenType.RBrace || kind == TokenType.EndOfFile || scanner.HasPrecedingLineBreak();
    }

    private bool TryParseSemicolon() {
        if (!CanParseSemicolon()) {
            return false;
        }

        if (currentToken.Type == TokenType.Semicolon) {
            // consume the semicolon if it was explicitly provided.
            NextToken();
        }

        return true;
    }

    private bool ParseSemicolon() {
        return TryParseSemicolon() || ParseExpected(TokenType.Semicolon);
    }

    public Statement ParsePackage() {
        if (currentToken.Type != TokenType.PackageKeyword) {
            Error($"expected package, but got {currentToken}");
        }

        var savedEnv = top;
        top = new Env(top);

        // Match package statement.
        ParseExpected(TokenType.PackageKeyword);
        var tok = currentToken;
        ParseExpected(TokenType.Identifier);
        var id = new Identifier(tok.Lexeme, TypeInfo.Package);
        top?.Add(tok.Lexeme, id);
        ParseSemicolon();

        // Toplevel statements.
        var list = ImportDeclarations();
        var varList = VarDeclarations();
        list.AddRange(varList);
        var funcList = FuncDeclarations();
        list.AddRange(funcList);

        var p = new PackageDeclaration(id.Name, list);
        top = savedEnv;
        return p;
    }

    public List<Statement> ImportDeclarations() {
        var list = new List<Statement>();
        while (currentToken.Type == TokenType.ImportKeyword) {
            NextToken();
            var tok = currentToken;
            ParseExpected(TokenType.StringLiteral);
            ParseSemicolon();
            var i = new ImportDeclaration(tok.Lexeme);
            list.Add(i);
        }
        return list;
    }

    public List<Statement> VarDeclarations() {
        var list = new List<Statement>();
        while (currentToken.Type == TokenType.VarKeyword) {
            var varStmt = ParseVar();
            list.Add(varStmt);
        }
        return list;
    }

    public Statement ParseVar() {
        ParseExpected(TokenType.VarKeyword);
        var tok = currentToken;
        ParseExpected(TokenType.Identifier);
        ParseExpected(TokenType.Colon);
        var p = GetTypeInfo();

        Expression? s = null;
        if (currentToken.Type != TokenType.Equal) {
            ParseSemicolon();
        } else {
            ParseExpected(TokenType.Equal);
            s = ParseExpression();
            ParseSemicolon();
        }
        var id = new Identifier(tok.Lexeme, p);
        top?.Add(tok.Lexeme, id);
        var VarStmt = new VariableDeclaration(id, s);
        return VarStmt;
    }

    // Get a type for the variable.
    private TypeInfo GetTypeInfo() {
        TypeInfo p;
        switch (currentToken.Type) {
        case TokenType.IntKeyword:
            p = TypeInfo.Int;
            NextToken();
            break;
        default:
            Error("no valid type");
            throw new Exception();
        }

        if (currentToken.Type != TokenType.LBracket) {
            return p;
        } else {
            return ParseArray(p);
        }
    }

    /// <summary>
    /// Array types.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    private TypeInfo ParseArray(TypeInfo p) {
        ParseExpected(TokenType.LBracket);
        var tok = currentToken;
        switch (tok.Type) {
        case TokenType.IntLiteral:
            NextToken();
            ParseExpected(TokenType.RBracket);
            return ParseIndexedAccess(p, tok.Lexeme);
        case TokenType.RBracket:
            NextToken();
            return ParseArrayType(p);
        default:
            Error("array type error");
            return null;
        }
    }

    private ArrayType ParseArrayType(TypeInfo a) {
        if (currentToken.Type == TokenType.LBracket) {
            NextToken();
            ParseExpected(TokenType.RBracket);
            a = ParseArrayType(a);
        }
        return new ArrayType(a);
    }

    private IndexedAccessType ParseIndexedAccess(TypeInfo p, string s) {
        if (currentToken.Type == TokenType.LBracket) {
            NextToken();
            var tok = currentToken;
            ParseExpected(TokenType.IntLiteral);
            ParseExpected(TokenType.RBracket);
            p = ParseIndexedAccess(p, tok.Lexeme);
        }
        return new IndexedAccessType(p, s);
    }

    private Expression ParseUnary() {
        if (unaryOperators.Contains(currentToken.Lexeme)) {
            NextToken();
            var op = currentToken;
            var operand = ParseUnary();
            return new UnaryExpression(op, operand);
        } else {
            return ParsePrimary();
        }
    }

    private int GetPrecedence(Token token) {
        if (binaryOperatorPrecedence.TryGetValue(token.Lexeme, out var p)) {
            return p;
        }

        return 0;
    }

    /// <summary>
    /// Factor.
    /// </summary>
    /// <returns></returns>
    private Expression ParsePrimary() {
        Expression x;
        switch (currentToken.Type) {
        case TokenType.LParen:
            // Ignore unnecessary parens.
            NextToken();
            x = ParseExpression();
            ParseExpected(TokenType.RParen);
            return x;
        case TokenType.IntLiteral:
            x = new IntLiteral(currentToken.Lexeme, currentToken.Pos);
            NextToken();
            return x;
        case TokenType.FloatLiteral:
            x = new FloatLiteral(currentToken.Lexeme, currentToken.Pos);
            NextToken();
            return x;
        case TokenType.StringLiteral:
            x = new StringLiteral(currentToken.Lexeme, currentToken.Pos);
            NextToken();
            return x;
        case TokenType.TrueKeyword:
            x = new BoolLiteral(currentToken.Lexeme, currentToken.Pos);
            NextToken();
            return x;
        case TokenType.FalseKeyword:
            x = new BoolLiteral(currentToken.Lexeme, currentToken.Pos);
            NextToken();
            return x;
        case TokenType.Identifier:
            return ParseAccessOrCall();
        case TokenType.LBracket:
            return ParseArrayLiteral();
        default:
            Error("Primary has a bug");
            // unreachable
            return null;
        }
    }

    public Expression ParseArrayLiteral() {
        ParseExpected(TokenType.LBracket);
        var list = new List<Expression>();
        var first = ParseUnary();
        list.Add(first);
        while (currentToken.Type == TokenType.Comma) {
            ParseExpected(TokenType.Comma);
            var elem = ParseUnary();
            list.Add(elem);
        }
        ParseExpected(TokenType.RBracket);
        return new ArrayLiteralExpression(list, 1);
    }

    private Expression ParseAccessOrCall() {
        var id = top?.Get(currentToken.Lexeme);
        if (id is null) {
            // Error($"{look} undeclared");
            // TODO: Handle undefined functions
            id = new Identifier(currentToken.Lexeme, TypeInfo.Func);
        }
        NextToken();

        if (id!.TypeInfo == TypeInfo.Func) {
            if (currentToken.Type != TokenType.LParen) {
                return id;
            } else {
                return ParseCall(id);
            }
        } else {
            if (currentToken.Type != TokenType.LBracket) {
                return id;
            } else {
                return ParseAccess(id);
            }
        }
    }

    // Actual arguments
    private List<Expression> ArgumentList() {
        var args = new List<Expression>();
        if (currentToken.Type == TokenType.RParen) {
            return args;
        }

        var first = ParseExpression();
        args.Add(first);

        ArgumentRest(args);
        return args;
    }

    private void ArgumentRest(List<Expression> args) {
        if (currentToken.Type != TokenType.Comma) {
            return;
        }

        ParseExpected(TokenType.Comma);
        var rest = ParseExpression();
        args.Add(rest);

        // Find the rest.
        ArgumentRest(args);
    }

    private CallExpression ParseCall(Expression expr) {
        ParseExpected(TokenType.LParen);
        var args = ArgumentList();
        ParseExpected(TokenType.RParen);
        var call = new CallExpression(expr, args);
        if (currentToken.Type == TokenType.LParen) {
            call = ParseCall(call);
        }

        return call;
    }

    public List<Statement> FuncDeclarations() {
        var list = new List<Statement>();
        while (currentToken.Type == TokenType.FuncKeyword) {
            var funcStmt = ParseFuncStatement();
            list.Add(funcStmt);
        }

        return list;
    }

    public FunctionDeclaration ParseFuncStatement() {
        var savedEnv = top;
        top = new Env(top);

        ParseExpected(TokenType.FuncKeyword);
        var tok = currentToken;
        ParseExpected(TokenType.Identifier);

        var funcId = new Identifier(tok.Lexeme, TypeInfo.Func);
        top?.Add(funcId.Name, funcId);

        ParseExpected(TokenType.LParen);
        var args = ParameterList();
        ParseExpected(TokenType.RParen);

        var returnType = ReturnType();
        var b = ParseBlock();
        return new FunctionDeclaration(funcId, args, returnType, b);
    }

    // Formal parameters
    private List<Identifier> ParameterList() {
        var args = new List<Identifier>();
        if (currentToken.Type == TokenType.RParen) {
            return args;
        }

        var tok = currentToken;
        ParseExpected(TokenType.Identifier);
        ParseExpected(TokenType.Colon);
        var p = GetTypeInfo();
        var id = new Identifier(tok.Lexeme, p);
        top?.Add(id.Name, id);
        args.Add(id);

        ParameterRest(args);
        return args;
    }

    private void ParameterRest(List<Identifier> args) {
        if (currentToken.Type != TokenType.Comma) {
            return;
        }

        ParseExpected(TokenType.Comma);
        var tok = currentToken;
        ParseExpected(TokenType.Identifier);
        ParseExpected(TokenType.Colon);
        var p = GetTypeInfo();
        var id = new Identifier(tok.Lexeme, p);
        top?.Add(id.Name, id);
        args.Add(id);

        // Find the rest parameters.
        ParameterRest(args);
    }

    private TypeInfo ReturnType() {
        if (currentToken.Type != TokenType.Arrow) {
            return new TypeInfo(TypeKind.Nil);
        }

        ParseExpected(TokenType.Arrow);
        var p = GetTypeInfo();
        return p;
    }

    public Block ParseBlock() {
        ParseExpected(TokenType.LBrace);
        var savedEnv = top;
        top = new Env(top);

        var list = new List<Statement>();
        var varDecls = VarDeclarations();
        list.AddRange(varDecls);

        var statements = StatementList();
        list.AddRange(statements);

        ParseExpected(TokenType.RBrace);
        top = savedEnv;
        return new Block(list);
    }

    private Expression ParseExpression(int precedence = 0) {
        var lhs = ParseUnary();

        while (precedence < GetPrecedence(currentToken)) {
            lhs = ParseBinary(lhs);
        }

        return lhs;
    }

    private Expression ParseBinary(Expression lhs) {
        var tok = currentToken;
        var precedence = GetPrecedence(tok);
        NextToken();
        var rhs = ParseExpression(precedence);
        return new BinaryExpression(tok, lhs, rhs);
    }

    private List<Statement> StatementList() {
        var list = new List<Statement>();
        while (currentToken.Type != TokenType.RBrace && currentToken.Type != TokenType.FuncKeyword && currentToken.Type != TokenType.EndOfFile) {
            var stmt = ParseStatement();
            if (stmt is not null) {
                list.Add(stmt);
            }
        }
        return list;
    }

    private Statement? ParseStatement() {
        switch (currentToken.Type) {
        case TokenType.Semicolon:
            NextToken();
            return null;
        case TokenType.WhileKeyword:
            NextToken();
            var whileExpr = ParseExpression();
            var whileBlock = ParseBlock();
            var whileStmt = new WhileStatement(whileExpr, whileBlock);
            return whileStmt;
        case TokenType.IfKeyword:
            return ParseIfStatement();
        case TokenType.BreakKeyword:
            NextToken();
            ParseSemicolon();
            return new BreakStatement();
        case TokenType.DoKeyword:
            NextToken();
            var doBlock = ParseBlock();
            ParseExpected(TokenType.WhileKeyword);
            var doExpr = ParseExpression();
            ParseSemicolon();
            return new DoStatement(doBlock, doExpr);
        case TokenType.LBrace:
            return ParseBlock();
        case TokenType.ReturnKeyword:
            return ParseReturn();
        case TokenType.VarKeyword:
            return ParseVar();
        case TokenType.Identifier:
            return ParseAssignOrCall();
        default:
            Error("unknown statement");
            return null;
        }
    }

    private Statement? ParseAssignOrCall() {
        var tok = currentToken;
        ParseExpected(TokenType.Identifier);
        var id = top?.Get(tok.Lexeme);
        if (id is null) {
            // Error($"{tok} undeclared");
            id = new Identifier(tok.Lexeme, TypeInfo.Func);
        }

        if (id.TypeInfo == TypeInfo.Func) {
            var call = ParseCall(id);
            return new ExpressionStatement(call);
        } else {
            return ParseAssign(id);
        }
    }

    private Statement ParseIfStatement() {
        ParseExpected(TokenType.IfKeyword);
        var expr = ParseExpression();
        var thenBlock = ParseBlock();
        if (currentToken.Type != TokenType.ElseKeyword) {
            return new IfStatement(expr, thenBlock);
        }
        ParseExpected(TokenType.ElseKeyword);

        if (currentToken.Type != TokenType.IfKeyword) {
            var elseBlock = ParseBlock();
            return new IfStatement(expr, thenBlock, elseBlock);
        }

        var ifStmt = ParseIfStatement();
        return new IfStatement(expr, thenBlock, ifStmt);
    }

    public Statement ParseReturn() {
        ParseExpected(TokenType.ReturnKeyword);

        if (CanParseSemicolon()) {
            return new ReturnStatement();
        } else {
            var s = ParseUnary();
            ParseSemicolon();
            return new ReturnStatement(s);
        }
    }

    private Statement? ParseAssign(Identifier id) {
        Statement? stmt = null;
        if (currentToken.Type == TokenType.Equal) {
            NextToken();
            stmt = new AssignStatement(id, ParseExpression());
        } else {
            // For array type
            var x = ParseAccess(id);
            ParseExpected(TokenType.Equal);
            stmt = new ElementAssignStatement(x, ParseExpression());
        }

        ParseSemicolon();
        return stmt;
    }

    public ElementAccessExpression ParseAccess(Expression expr) {
        ParseExpected(TokenType.LBracket);
        var index = ParseExpression();
        ParseExpected(TokenType.RBracket);
        var access = new ElementAccessExpression(expr, index);
        if (currentToken.Type == TokenType.LBracket) {
            access = ParseAccess(access);
        }

        return access;
    }
}
