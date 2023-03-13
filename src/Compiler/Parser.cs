using Gaia.AST;

namespace Gaia.Compiler;

public class Parser {
    private readonly Scanner lex;
    private Token look;
    private Env? top = null;
    private readonly Dictionary<string, int> binaryOperatorPrecedence = new() {
        { "=", 80 },
        {"||", 70},
        {"&&", 60},
        {"|",50},
        {"&",40},
        { "==", 30 },
        { "!=", 30 },
        { "<", 20 },
        { "<=", 10 },
        { ">", 10 },
        { ">=", 10 },
        { "+", 2 },
        { "-", 2 },
        { "*", 1 },
        { "/", 1 },
        { "%", 1 },
    };
    private readonly HashSet<string> unaryOperators = new HashSet<string>() { "-" };

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
        Match(TokenType.Id);
        var id = new IdNode(tok.Lexeme, IdType.Package);
        top?.Add(tok.Lexeme, id);
        Match(TokenType.Semicolon);

        // Toplevel statements.
        var exprList = VarDeclarations();
        var f = FuncDeclarations();
        exprList.AddRange(f);

        var p = new PackageNode(id.Name, exprList);
        top = savedEnv;
        return p;
    }

    public List<Node> VarDeclarations() {
        var exprList = new List<Node>();
        while (look.Type == TokenType.Var) {
            Match(TokenType.Var);
            var tok = look;
            Match(TokenType.Id);
            Match(TokenType.Colon);
            var p = GetIdType();

            Node? s = null;
            if (look.Type != TokenType.Equal) {
                Match(TokenType.Semicolon);
            } else {
                Advance();
                s = Unary();
                Match(TokenType.Semicolon);
            }
            var id = new IdNode(tok.Lexeme, p);
            top?.Add(tok.Lexeme, id);
            var varNode = new VarAssignNode(id, s);
            exprList.Add(varNode);
        }
        return exprList;
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

    public Node Unary() {
        if (unaryOperators.Contains(look.Lexeme)) {
            Advance();
            var operand = Unary();
            return new UnaryNode(look, operand);
        }
        /* else if (look.Tag == '!') {
            var tok = look;
            Move();
            return new Not(tok, Unary());
        }
        */ else {
            return Primary();
        }
    }

    /// <summary>
    /// Smallest factor.
    /// </summary>
    /// <returns></returns>
    public Node Primary() {
        Node x;
        switch (look.Type) {
        case TokenType.IntLiteral:
            x = new IntLiteralNode(look.Lexeme, (int)look.Value!);
            Advance();
            return x;
        /*
    case '(':
        // Ignore unnecessary parens.
        Move();
        x = Bool();
        Match(')');
        return x;
    case Tag.Real:
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
    case Tag.Id:
        var id = top?.Get(look);
        if (id is null) {
            Error($"{look} undeclared");
        }
        Move();
        if (look.Tag != '[') {
            return id!;
        } else {
            return Offset(id!);
        }
        */
        default:
            Error("Factor has a bug");
            // unreachable
            return null;
        }
    }

    public List<Node> FuncDeclarations() {
        if (look.Type != TokenType.Func) {
            Error("func expected");
        }

        var list = new List<Node>();
        var savedEnv = top;
        top = new Env(top);

        Match(TokenType.Func);
        var tok = look;
        Match(TokenType.Id);

        var funcId = new IdNode(tok.Lexeme, IdType.Func);
        top?.Add(funcId.Name, funcId);

        Match(TokenType.LParen);
        var args = ArgList();
        Match(TokenType.RParen);

        var returnType = ReturnType();
        var b = Block();
        var funcNode = new FuncNode(funcId.Name, args, returnType, list);
        list.Add(funcNode);
        return list;
    }

    public List<IdNode> ArgList() {
        var args = new List<IdNode>();
        if (look.Type == TokenType.RParen) {
            return args;
        }

        var tok = look;
        Match(TokenType.Id);
        Match(TokenType.Colon);
        var p = GetIdType();
        var id = new IdNode(tok.Lexeme, p);
        top?.Add(id.Name, id);
        args.Add(id);

        ArgRest(args);

        return args;
    }

    public void ArgRest(List<IdNode> args) {
        if (look.Type != TokenType.Comma) {
            return;
        }
        Advance();

        var tok = look;
        Match(TokenType.Id);
        Match(TokenType.Colon);
        var p = GetIdType();
        var id = new IdNode(tok.Lexeme, p);
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

    public List<Node> Block() {
        Match(TokenType.LBrace);
        var savedEnv = top;
        top = new Env(top);
        // VarDecls();
        // var s = Stmts();
        Match(TokenType.RBrace);
        top = savedEnv;
        // return s;
        return new List<Node>();
    }

    /*
        public Stmt Stmts() {
            switch (look.Tag) {
            case '}':
            case Tag.Func:
            case Tag.Eof:
                return Inter.Stmt.Null;
            default:
                return new Seq(Stmt(), Stmts());
            }
        }

        public Stmt Stmt() {
            Expr x;
            Stmt s, s1, s2;
            Stmt savedStmt;

            switch (look.Tag) {
            case ';':
                Move();
                return Inter.Stmt.Null;
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
            case Tag.Loop:
                var loopNode = new Loop();
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
            case '{':
                return Block();
            case Tag.Ret:
                return ReturnStmt();
            default:
                return Assign();
            }
        }

        public Stmt ReturnStmt() {
            Match(Tag.Ret);
            var t = look;
            Match(Tag.Id);
            var id = top?.Get(t);
            if (id is null) {
                Error($"{id} undeclared");
            }
            Match(';');
            return new Ret();
        }

        // Return expressions, and bool is the highest precedence.
        public Expr Bool() {
            var x = Join();
            while (look.Tag == Tag.Or) {
                var tok = look;
                Move();
                x = new Or(tok, x, Join());
            }
            return x;
        }

        public Expr Join() {
            var x = Equality();
            while (look.Tag == Tag.And) {
                var tok = look;
                Move();
                x = new And(tok, x, Equality());
            }
            return x;
        }

        public Expr Equality() {
            var x = Rel();
            while (look.Tag == Tag.Eq || look.Tag == Tag.Ne) {
                var tok = look;
                Move();
                x = new Rel(tok, x, Rel());
            }
            return x;
        }

        public Expr Rel() {
            var x = Expr();
            switch (look.Tag) {
            case '<':
            case Tag.Le:
            case Tag.Ge:
            case '>':
                var tok = look;
                Move();
                return new Rel(tok, x, Expr());
            default:
                return x;
            }
        }

        public Expr Expr() {
            var x = Term();
            while (look.Tag == '+' || look.Tag == '-') {
                var tok = look;
                Move();
                x = new Arith(tok, x, Term());
            }
            return x;
        }

        public Stmt Assign() {
            var stmt = Inter.Stmt.Null;
            var t = look;
            Match(Tag.Id);
            var id = top?.Get(t);
            if (id is null) {
                Error($"{t} undeclared");
            }
            if (look.Tag == '=') {
                Move();
                stmt = new Set(id!, Bool());
            } else {
                // For array
                var x = Offset(id);
                Match('=');
                stmt = new SetElem(x, Bool());
            }

            Match(';');
            return stmt;
        }

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
