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
    private Token Advance() {
        look = lex.Scan();
        return look;
    }

    public static void Error(string s) {
        throw new Exception($"Near line {Scanner.Line} position {Scanner.Pos}: {s}.");
    }

    /// <summary>
    /// Match and move to the next token.
    /// </summary>
    /// <param name="t"></param>
    private void Match(TokenType t) {
        if (look.Type == t) {
            Advance();
        } else {
            Error($"token unmatched: {look} != {t}");
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
        var id = new Identifier(tok.Lexeme, IdType.Package);
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
            var t = look;
            Match(TokenType.StringLiteral);
            Match(TokenType.Semicolon);
            var i = new ImportDeclaration(t.Lexeme);
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
        var p = GetIdType();

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
    private IdType GetIdType() {
        var p = look.Type;
        switch (p) {
        case TokenType.IntKeyword:
            Advance();
            return IdType.Int;
        default:
            Error("no valid type");
            throw new Exception();
        }
        // if (look.Tag != '[') {
        //     return p;
        // } else {
        //     return Dims(p);
        // }
    }

    private Expression ParseUnary() {
        if (unaryOperators.Contains(look.Lexeme)) {
            var op = Advance();
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
        case TokenType.IntLiteral:
            x = new IntLiteral(look.Lexeme, (int)look.Value!);
            Advance();
            return x;
        /*
    case '(':
        // Ignore unnecessary parens.
        Move();
        x = Bool();
        Match(')');
        return x;
    case Tag.Float:
        x = new Constant(look, Typing.Float64);
        Move();
        return x;
    case Tag.True:
        x = Constant.True;
        Move();
        return x;
    case Tag.False:
        x = Constant.False;
        Move();
        return x;

        */
        case TokenType.Identifier:
            var id = top?.Get(look.Lexeme);
            if (id is null) {
                Error($"{look} undeclared");
            }
            Advance();
            if (look.Type != TokenType.LBracket) {
                return id!;
            } else {
                // return Offset(id!);
                return null;
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

        var funcId = new Identifier(tok.Lexeme, IdType.Func);
        top?.Add(funcId.Name, funcId);

        Match(TokenType.LParen);
        var args = ParameterList();
        Match(TokenType.RParen);

        var returnType = ReturnType();
        var b = ParseBlock();
        return new FunctionDeclaration(funcId.Name, args, returnType, b);
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
        var p = GetIdType();
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
        var t = look;
        Match(TokenType.Identifier);
        Match(TokenType.Colon);
        var p = GetIdType();
        var id = new Identifier(t.Lexeme, p);
        top?.Add(id.Name, id);
        args.Add(id);

        // Find the rest parameters.
        ParameterRest(args);
    }

    private IdType ReturnType() {
        if (look.Type != TokenType.Arrow) {
            return IdType.Nil;
        }

        Match(TokenType.Arrow);
        var p = GetIdType();
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
        var token = look;
        var precedence = GetPrecedence(token);
        Advance();
        var rhs = ParseExpression(precedence);
        return new BinaryExpression(token, lhs, rhs);
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
            var x = ParseExpression();
            var b = ParseBlock();
            var stmt = new WhileStatement(x, b);
            return stmt;
        /*
        case Tag.If:
            Match(Tag.If);
            x = Bool();
            s1 = Block();
            if (look.Tag != Tag.Else) {
                return new If(x, s1);
            }
            Match(Tag.Else);
            s2 = Block();
            return new Else(x, s1, s2);
        case Tag.Break:
            Match(Tag.Break);
            Match(';');
            return new Break();
            */
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
        var t = look;
        Match(TokenType.Identifier);
        var id = top?.Get(t.Lexeme);
        if (id is null) {
            Error($"{t} undeclared");
        }
        if (look.Type == TokenType.Equal) {
            Advance();
            stmt = new AssignStatement(id!, ParseExpression());
        } else {
            // For array
            // var x = Offset(id);
            // Match('=');
            // stmt = new SetElem(x, Bool());
        }

        Match(TokenType.Semicolon);
        return stmt;
    }

    /*
            public Typing Dims(Typing p) {
                Match('[');
                var tok = look;
                Match(Tag.Num);
                Match(']');
                if (look.Tag == '[') {
                    p = Dims(p);
                }
                return new Arr(((Num)tok).Value, p);
            }

            public Expr Term() {
                var x = Unary();
                while (look.Tag == '*' || look.Tag == '/') {
                    var tok = look;
                    Move();
                    x = new Arith(tok, x, Unary());
                }
                return x;
            }

            public Access Offset(Id a) {
                Expr i;
                Expr w;
                Expr t1, t2;
                Expr loc;
                var type = a.Typ;
                Match('[');
                i = Bool();
                Match(']');
                type = ((Arr)type).Of;
                w = new Constant(type.Width);
                t1 = new Arith(new Token('*'), i, w);
                loc = t1;
                while (look.Tag == '[') {
                    Match('[');
                    i = Bool();
                    Match(']');
                    w = new Constant(type.Width);
                    t1 = new Arith(new Token('*'), i, w);
                    t2 = new Arith(new Token('+'), loc, t1);
                    loc = t2;
                }
                return new Access(a, loc, type);
            }
            */
}
