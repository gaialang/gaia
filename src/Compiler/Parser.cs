using Gaia.AST;

namespace Gaia.Compiler;

public sealed class Parser {
    private readonly Scanner lex;
    private Token look;
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

        { ",", 1},
    };
    private readonly HashSet<string> unaryOperators = new HashSet<string>() { "-", "!" };

    public Parser(Scanner lex) {
        this.lex = lex;
        look = this.lex.Scan();
    }

    public Node Parse() {
        return ParsePackage();
    }

    /// <summary>
    /// Return the next token.
    /// </summary>
    /// <returns></returns>
    private void Advance() {
        look = lex.Scan();
    }

    public static void Error(string s) {
        throw new Exception($"Near line {Scanner.Line} column {Scanner.Column}: {s}.");
    }

    /// <summary>
    /// Match and move to the next token.
    /// </summary>
    /// <param name="t"></param>
    private void Match(TokenType t) {
        if (look.Type == t) {
            Advance();
        } else {
            Error($"expected {t}, but got {look}");
        }
    }

    public Statement ParsePackage() {
        if (look.Type != TokenType.PackageKeyword) {
            Error("Expected package");
        }

        var savedEnv = top;
        top = new Env(top);

        // Match package statement.
        Match(TokenType.PackageKeyword);
        var tok = look;
        Match(TokenType.Identifier);
        var id = new Identifier(tok.Lexeme, TypeInfo.Package);
        top?.Add(tok.Lexeme, id);
        Match(TokenType.Semicolon);

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
        while (look.Type == TokenType.ImportKeyword) {
            Advance();
            var tok = look;
            Match(TokenType.StringLiteral);
            Match(TokenType.Semicolon);
            var i = new ImportDeclaration(tok.Lexeme);
            list.Add(i);
        }
        return list;
    }

    public List<Statement> VarDeclarations() {
        var list = new List<Statement>();
        while (look.Type == TokenType.VarKeyword) {
            var varStmt = ParseVar();
            list.Add(varStmt);
        }
        return list;
    }

    public Statement ParseVar() {
        Match(TokenType.VarKeyword);
        var tok = look;
        Match(TokenType.Identifier);
        Match(TokenType.Colon);
        var p = GetTypeInfo();

        Expression? s = null;
        if (look.Type != TokenType.Equal) {
            Match(TokenType.Semicolon);
        } else {
            Match(TokenType.Equal);
            s = ParseExpression();
            Match(TokenType.Semicolon);
        }
        var id = new Identifier(tok.Lexeme, p);
        top?.Add(tok.Lexeme, id);
        var VarStmt = new VariableDeclaration(id, s);
        return VarStmt;
    }

    // Get a type for the variable.
    private TypeInfo GetTypeInfo() {
        TypeInfo p;
        switch (look.Type) {
        case TokenType.IntKeyword:
            p = TypeInfo.Int;
            Advance();
            break;
        default:
            Error("no valid type");
            throw new Exception();
        }

        if (look.Type != TokenType.LBracket) {
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
        Match(TokenType.LBracket);
        var tok = look;
        switch (tok.Type) {
        case TokenType.IntLiteral:
            Advance();
            Match(TokenType.RBracket);
            return ParseIndexedAccess(p, tok.Lexeme);
        case TokenType.RBracket:
            Advance();
            return ParseArrayType(p);
        default:
            Error("array type error");
            return null;
        }
    }

    private ArrayType ParseArrayType(TypeInfo a) {
        if (look.Type == TokenType.LBracket) {
            Advance();
            Match(TokenType.RBracket);
            a = ParseArrayType(a);
        }
        return new ArrayType(a);
    }

    private IndexedAccessType ParseIndexedAccess(TypeInfo p, string s) {
        if (look.Type == TokenType.LBracket) {
            Advance();
            var tok = look;
            Match(TokenType.IntLiteral);
            Match(TokenType.RBracket);
            p = ParseIndexedAccess(p, tok.Lexeme);
        }
        return new IndexedAccessType(p, s);
    }

    private Expression ParseUnary() {
        if (unaryOperators.Contains(look.Lexeme)) {
            Advance();
            var op = look;
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
        switch (look.Type) {
        case TokenType.LParen:
            // Ignore unnecessary parens.
            Advance();
            x = ParseExpression();
            Match(TokenType.RParen);
            return x;
        case TokenType.IntLiteral:
            x = new IntLiteral(look.Lexeme, look.Pos);
            Advance();
            return x;
        case TokenType.FloatLiteral:
            x = new FloatLiteral(look.Lexeme, look.Pos);
            Advance();
            return x;
        case TokenType.TrueKeyword:
            x = new BoolLiteral(look.Lexeme);
            Advance();
            return x;
        case TokenType.FalseKeyword:
            x = new BoolLiteral(look.Lexeme);
            Advance();
            return x;
        case TokenType.Identifier:
            var id = top?.Get(look.Lexeme);
            if (id is null) {
                Error($"{look} undeclared");
            }
            Advance();

            if (look.Type != TokenType.LBracket) {
                return id!;
            } else {
                return Offset(id!);
            }
        default:
            Error("Primary has a bug");
            // unreachable
            return null;
        }
    }

    public List<Statement> FuncDeclarations() {
        var list = new List<Statement>();
        while (look.Type == TokenType.FuncKeyword) {
            var funcStmt = ParseFuncStatement();
            list.Add(funcStmt);
        }

        return list;
    }

    public FunctionDeclaration ParseFuncStatement() {
        var savedEnv = top;
        top = new Env(top);

        Match(TokenType.FuncKeyword);
        var tok = look;
        Match(TokenType.Identifier);

        var funcId = new Identifier(tok.Lexeme, TypeInfo.Func);
        top?.Add(funcId.Name, funcId);

        Match(TokenType.LParen);
        var args = ParameterList();
        Match(TokenType.RParen);

        var returnType = ReturnType();
        var b = ParseBlock();
        return new FunctionDeclaration(funcId, args, returnType, b);
    }

    // Parameters
    private List<Identifier> ParameterList() {
        var args = new List<Identifier>();
        if (look.Type == TokenType.RParen) {
            return args;
        }

        var tok = look;
        Match(TokenType.Identifier);
        Match(TokenType.Colon);
        var p = GetTypeInfo();
        var id = new Identifier(tok.Lexeme, p);
        top?.Add(id.Name, id);
        args.Add(id);

        ParameterRest(args);
        return args;
    }

    private void ParameterRest(List<Identifier> args) {
        if (look.Type != TokenType.Comma) {
            return;
        }

        Match(TokenType.Comma);
        var tok = look;
        Match(TokenType.Identifier);
        Match(TokenType.Colon);
        var p = GetTypeInfo();
        var id = new Identifier(tok.Lexeme, p);
        top?.Add(id.Name, id);
        args.Add(id);

        // Find the rest parameters.
        ParameterRest(args);
    }

    private TypeInfo ReturnType() {
        if (look.Type != TokenType.Arrow) {
            return new TypeInfo(TypeKind.Nil);
        }

        Match(TokenType.Arrow);
        var p = GetTypeInfo();
        return p;
    }

    public Block ParseBlock() {
        Match(TokenType.LBrace);
        var savedEnv = top;
        top = new Env(top);

        var list = new List<Statement>();
        var varDecls = VarDeclarations();
        list.AddRange(varDecls);

        var statements = StatementList();
        list.AddRange(statements);

        Match(TokenType.RBrace);
        top = savedEnv;
        return new Block(list);
    }

    private Expression ParseExpression(int precedence = 0) {
        var lhs = ParseUnary();

        while (precedence < GetPrecedence(look)) {
            lhs = ParseBinary(lhs);
        }

        return lhs;
    }

    private Expression ParseBinary(Expression lhs) {
        var tok = look;
        var precedence = GetPrecedence(tok);
        Advance();
        var rhs = ParseExpression(precedence);
        return new BinaryExpression(tok, lhs, rhs);
    }

    private List<Statement> StatementList() {
        var list = new List<Statement>();
        while (look.Type != TokenType.RBrace && look.Type != TokenType.FuncKeyword && look.Type != TokenType.EndOfFile) {
            var stmt = ParseStatement();
            if (stmt is not null) {
                list.Add(stmt);
            }
        }
        return list;
    }

    private Statement? ParseStatement() {
        switch (look.Type) {
        case TokenType.Semicolon:
            Advance();
            return null;
        case TokenType.WhileKeyword:
            Advance();
            var whileExpr = ParseExpression();
            var whileBlock = ParseBlock();
            var whileStmt = new WhileStatement(whileExpr, whileBlock);
            return whileStmt;
        case TokenType.IfKeyword:
            return ParseIfStatement();
        case TokenType.BreakKeyword:
            Advance();
            Match(TokenType.Semicolon);
            return new BreakStatement();
        case TokenType.DoKeyword:
            Advance();
            var doBlock = ParseBlock();
            Match(TokenType.WhileKeyword);
            var doExpr = ParseExpression();
            Match(TokenType.Semicolon);
            return new DoStatement(doBlock, doExpr);
        case TokenType.LBrace:
            return ParseBlock();
        case TokenType.ReturnKeyword:
            return ParseReturn();
        case TokenType.VarKeyword:
            return ParseVar();
        default:
            return ParseAssign();
        }
    }

    private Statement ParseIfStatement() {
        Match(TokenType.IfKeyword);
        var expr = ParseExpression();
        var thenBlock = ParseBlock();
        if (look.Type != TokenType.ElseKeyword) {
            return new IfStatement(expr, thenBlock);
        }
        Match(TokenType.ElseKeyword);

        if (look.Type != TokenType.IfKeyword) {
            var elseBlock = ParseBlock();
            return new IfStatement(expr, thenBlock, elseBlock);
        }

        var ifStmt = ParseIfStatement();
        return new IfStatement(expr, thenBlock, ifStmt);
    }

    public Statement ParseReturn() {
        Match(TokenType.ReturnKeyword);

        if (look.Type != TokenType.Semicolon) {
            var s = ParseUnary();
            return new ReturnStatement(s);
        }

        Match(TokenType.Semicolon);
        return new ReturnStatement();
    }

    private Statement? ParseAssign() {
        Statement? stmt = null;
        var tok = look;
        Match(TokenType.Identifier);
        var id = top?.Get(tok.Lexeme);
        if (id is null) {
            Error($"{tok} undeclared");
        }
        if (look.Type == TokenType.Equal) {
            Advance();
            stmt = new AssignStatement(id!, ParseExpression());
        } else {
            // For array
            var x = Offset(id!);
            Match(TokenType.Equal);
            stmt = new ElementAssignStatement(x, ParseExpression());
        }

        Match(TokenType.Semicolon);
        return stmt;
    }

    public ElementAccessExpression Offset(Identifier id) {
        ElementAccessExpression access;

        do {
            Match(TokenType.LBracket);
            var index = ParseExpression();
            Match(TokenType.RBracket);
            access = new ElementAccessExpression(id, index);
        } while (look.Type == TokenType.LBracket);

        return access;
    }
}
