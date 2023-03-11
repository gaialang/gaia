using Gaia.AST;

namespace Gaia.Compiler;

public class Parser {
    private readonly Scanner lex;
    private Token look;
    private Env? top = null;
    private readonly Dictionary<string, int> binaryOperatorPrecedence = new() {
        { "=", 2 },
        {"||", 10},
        {"&&", 20},
        {"|",30},
        {"&",40},
        { "==", 50 },
        { "!=", 50 },
        { "<", 60 },
        { "<=", 60 },
        { ">", 60 },
        { ">=", 60 },
        { "+", 70 },
        { "-", 70 },
        { "*", 80 },
        { "/", 80 },
        { "%", 80 },
    };
    private readonly HashSet<string> unaryOperators = new HashSet<string>() { "-" };

    public Parser(Scanner lex) {
        this.lex = lex;
        look = this.lex.Scan();
    }

    public Node Parse() {
        return ParsePackage();
    }

    public void Advance() {
        look = lex.Scan();
    }

    public static void Error(string s) {
        throw new Exception($"Near line {Scanner.Line} position {Scanner.Pos}: {s}.");
    }

    /// <summary>
    /// Match and move to next token.
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

        // Match package clause.
        Match(TokenType.Package);
        var tok = look;
        Match(TokenType.Id);
        var id = new IdNode(tok.Lexeme, IdType.Package);
        top?.Add(tok.Lexeme, id);
        Match(TokenType.Semicolon);

        var exprList = VarDeclarations();
        // var f = FuncDecl();

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
            var varNode = new VarNode(id, s);
            exprList.Add(varNode);
        }
        return exprList;
    }

    // Get a type for the variable.
    public IdType GetIdType() {
        var ty = look.Type;
        switch (ty) {
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
            return Factor();
        }
    }

    public Node Factor() {
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

    /*
        public Stmt Block() {
            Match('{');
            var savedEnv = top;
            top = new Env(top);
            VarDecls();
            var s = Stmts();
            Match('}');
            top = savedEnv;
            return s;
        }

        public Function FuncDecl() {
            if (look.Tag != Tag.Func) {
                Error("func expected");
            }

            var savedEnv = top;
            top = new Env(top);
            Match(Tag.Func);
            Match(Tag.Id);

            var tok = look;
            var funcName = new Id(tok, Typing.Func, used);
            top?.Add(tok, funcName);
            used += Typing.Func.Width;

            Match('(');
            var args = ArgList();
            Match(')');

            var retType = ReturnType();
            var stmt = Block();
            return new Function(funcName, args, Typing.Int, stmt);
        }

        public Typing ReturnType() {
            if (look.Tag != ':') {
                return Typing.Nil;
            }
            Match(':');
            var p = GetTyping();
            return p;
        }

        public List<Id> ArgList() {
            var args = new List<Id>();
            if (look.Tag == ')') {
                return args;
            }

            var tok = look;
            Match(Tag.Id);
            Match(':');
            var p = GetTyping();
            var id = new Id(tok, p, used);
            top?.Add(tok, id);
            used += Typing.Int.Width;
            args.Add(id);

            ArgRest(args);

            return args;
        }

        public void ArgRest(List<Id> args) {
            if (look.Tag != ',') {
                return;
            }

            Match(',');
            var tok = look;
            Match(Tag.Id);
            Match(':');
            var p = GetTyping();
            var id = new Id(tok, p, used);
            top?.Add(tok, id);
            used += Typing.Int.Width;
            args.Add(id);

            // Find the rest arguments.
            ArgRest(args);
        }

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
