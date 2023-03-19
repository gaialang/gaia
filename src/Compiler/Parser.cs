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
    public Token Advance() {
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
    public void Match(TokenType t) {
        if (look.Type == t) {
            Advance();
        } else {
            Error($"token unmatched: {look} != {t}");
        }
    }

    public Node ParsePackage() {
        if (look.Type != TokenType.Package) {
            Error("Expected package");
        }

        var savedEnv = top;
        top = new Env(top);

        // Match package statement.
        Match(TokenType.Package);
        var tok = look;
        Match(TokenType.Identifier);
        var id = new Identifier(tok.Lexeme, IdType.Package);
        top?.Add(tok.Lexeme, id);
        Match(TokenType.Semicolon);

        // Toplevel statements.
        var exprList = VarDeclarations();
        var f = FuncDeclarations();
        exprList.AddRange(f);

        var p = new PackageStmt(id.Name, exprList);
        top = savedEnv;
        return p;
    }

    public List<Stmt> VarDeclarations() {
        var list = new List<Stmt>();
        while (look.Type == TokenType.Var) {
            Match(TokenType.Var);
            var tok = look;
            Match(TokenType.Identifier);
            Match(TokenType.Colon);
            var p = GetIdType();

            Expr? s = null;
            if (look.Type != TokenType.Equal) {
                Match(TokenType.Semicolon);
            } else {
                Advance();
                s = Expression();
                Match(TokenType.Semicolon);
            }
            var id = new Identifier(tok.Lexeme, p);
            top?.Add(tok.Lexeme, id);
            var varNode = new VarStmt(id, s);
            list.Add(varNode);
        }
        return list;
    }

    // Get a type for the variable.
    public IdType GetIdType() {
        var p = look.Type;
        switch (p) {
        case TokenType.Int:
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

    public Expr Unary() {
        if (unaryOperators.Contains(look.Lexeme)) {
            var op = Advance();
            var operand = Unary();
            return new UnaryOpExpr(op, operand);
        } else {
            return Primary();
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
    public Expr Primary() {
        Expr x;
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

    public List<Stmt> FuncDeclarations() {
        if (look.Type != TokenType.Func) {
            Error("func expected");
        }

        var list = new List<Stmt>();
        var savedEnv = top;
        top = new Env(top);

        Match(TokenType.Func);
        var tok = look;
        Match(TokenType.Identifier);

        var funcId = new Identifier(tok.Lexeme, IdType.Func);
        top?.Add(funcId.Name, funcId);

        Match(TokenType.LParen);
        var args = ArgList();
        Match(TokenType.RParen);

        var returnType = ReturnType();
        var b = Block();
        var funcNode = new FuncStmt(funcId.Name, args, returnType, b);
        list.Add(funcNode);
        return list;
    }

    public List<Identifier> ArgList() {
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

        ArgRest(args);

        return args;
    }

    public void ArgRest(List<Identifier> args) {
        if (look.Type != TokenType.Comma) {
            return;
        }
        Advance();

        var tok = look;
        Match(TokenType.Identifier);
        Match(TokenType.Colon);
        var p = GetIdType();
        var id = new Identifier(tok.Lexeme, p);
        top?.Add(id.Name, id);
        args.Add(id);

        // Find the rest arguments.
        ArgRest(args);
    }

    public IdType ReturnType() {
        if (look.Type != TokenType.Arrow) {
            return IdType.Nil;
        }
        Advance();
        var p = GetIdType();
        return p;
    }

    public StmtList? Block() {
        Match(TokenType.LBrace);
        var savedEnv = top;
        top = new Env(top);
        var varDecls = VarDeclarations();
        var s = StatementList();
        Match(TokenType.RBrace);
        top = savedEnv;
        return s;
    }

    public Expr Expression(int precedence = 0) {
        var lhs = Unary();

        while (precedence < GetPrecedence(look)) {
            lhs = ParseBinary(lhs);
        }

        return lhs;
    }

    private Expr ParseBinary(Expr lhs) {
        var token = look;
        var precedence = GetPrecedence(token);
        Advance();
        var rhs = Expression(precedence);
        return new BinaryOpExpr(token, lhs, rhs);
    }

    public StmtList? StatementList() {
        switch (look.Type) {
        case TokenType.RBrace:
        case TokenType.Func:
        case TokenType.EndOfFile:
            return null;
        default:
            return new StmtList(Statement(), StatementList());
        }
    }

    public Stmt? Statement() {
        Expr x;
        Stmt? s1, s2;
        Stmt? savedStmt;

        switch (look.Type) {
        case TokenType.Semicolon:
            Advance();
            return null;
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
        case Tag.While:
            var whileNode = new While();
            savedStmt = Inter.Stmt.Enclosing;
            Inter.Stmt.Enclosing = whileNode;
            Match(Tag.While);
            x = Bool();
            s1 = Stmt();
            whileNode.Init(x, s1);
            Inter.Stmt.Enclosing = savedStmt;
            return whileNode;
        case Tag.Do:
            var loopNode = new DoNode();
            savedStmt = Inter.Stmt.Enclosing;
            Inter.Stmt.Enclosing = loopNode;
            Match(Tag.Loop);
            s1 = Block();
            Match(Tag.While);
            x = Bool();
            Match(';');
            loopNode.Init(s1, x);
            Inter.Stmt.Enclosing = savedStmt;
            return loopNode;
        case Tag.Break:
            Match(Tag.Break);
            Match(';');
            return new Break();
            */
        case TokenType.LBrace:
            return Block();
        // case TokenType.Return:
        //     return ReturnStmt();
        default:
            return Assign();
        }
    }

    // public Stmt ReturnStmt() {
    //     Match(TokenType.Return);
    //     var t = look;
    //     Match(TokenType.Id);
    //     var id = top?.Get(t.Lexeme);
    //     if (id is null) {
    //         Error($"{t} undeclared");
    //     }
    //     Match(TokenType.Semicolon);
    //     return new ReturnStmt();
    // }

    public Stmt? Assign() {
        Stmt? stmt = null;
        var t = look;
        Match(TokenType.Identifier);
        var id = top?.Get(t.Lexeme);
        if (id is null) {
            Error($"{t} undeclared");
        }
        if (look.Type == TokenType.Equal) {
            Advance();
            stmt = new AssignStmt(id!, Expression());
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
