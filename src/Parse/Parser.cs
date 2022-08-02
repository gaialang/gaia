namespace Gaia.Parse;

using System;
using System.Collections.Generic;
using System.Data;
using Gaia.Inter;
using Gaia.Lex;
using Gaia.Symbols;

public class Parser {
    private Lexer lexer;
    private Token look;
    private Env? top;
    private int used = 0;

    public Parser(Lexer l) {
        lexer = l;
        look = lexer.Scan();
    }

    public void Run() {
        Package();
        Console.WriteLine("OK.");
    }

    public void Move() {
        look = lexer.Scan();
    }

    public void Error(string s) {
        throw new SyntaxErrorException($"Near line {Lexer.Line} position {Lexer.Pos}: {s}.");
    }

    /// <summary>
    /// Match and move to next token.
    /// </summary>
    /// <param name="t"></param>
    public void Match(int t) {
        if (look.Tag == t) {
            Move();
        }
        else {
            Error("Syntax error.");
        }
    }

    public void Package() {
        if (look.Tag != Tag.Pkg) {
            Error("Expected package.");
        }

        var savedEnv = top;
        top = new Env(top);

        // Match package clause.
        Match(Tag.Pkg);
        var tok = look;
        Match(Tag.Id);
        var id = new Id((Word)tok, Typ.Pkg, used);
        top?.Add(tok, id);
        used += Typ.Pkg.Width;
        Match(Tag.Semicolon);

        Decls();
        var s = Stmts();
        var f = Func();

        var p = new Pkg(id);
        top = savedEnv;
    }

    public Stmt FuncBlock() {
        Match(Tag.LBrac);
        Decls();

        var s = Stmts();
        Match(Tag.Ret);
        var t = look;
        Match(Tag.Id);

        var id = top?.Get(t);
        if (id is null) {
            Error($"{id} undeclared.");
        }

        Match(Tag.Semicolon);
        Match(Tag.RBrac);
        return s;
    }

    // TODO:
    public Function Func() {
        if (look.Tag != Tag.Func) {
            Error("Expected func.");
        }

        var savedEnv = top;
        top = new Env(top);
        Match(Tag.Func);
        var tok = look;
        Match(Tag.Id);

        var funcName = new Id((Word)tok, Typ.Func, used);
        top?.Add(tok, funcName);
        used += Typ.Func.Width;

        var args = Args();

        Match(Tag.Colon);
        Match(Tag.Int);

        var stmt = FuncBlock();

        return new Function(funcName, args, Typ.Int, stmt);
    }

    public List<Id> Args() {
        var argsList = new List<Id>();

        Match(Tag.LParen);

        var tok = look;
        Match(Tag.Id);
        var id = new Id((Word)tok, Typ.Int, used);
        top?.Add(tok, id);
        used += Typ.Int.Width;
        argsList.Add(id);

        Match(Tag.Colon);
        Match(Tag.Int);
        Match(Tag.RParen);

        return argsList;
    }

    public Stmt Stmts() {
        // TODO: Fix exit conditions
        if (look.Tag == Tag.Func || look.Tag == Tag.EOF || look.Tag == Tag.Ret) {
            return Inter.Stmt.Null;
        }
        else {
            return new Seq(Stmt(), Stmts());
        }
    }

    public Stmt Stmt() {
        Expr x;
        Stmt s, s1, s2;
        Stmt savedStmt;

        switch (look.Tag) {
        case Tag.Semicolon:
            Move();
            return Inter.Stmt.Null;
        case Tag.If:
            Match(Tag.If);
            x = Bool();
            s1 = Stmt();
            if (look.Tag != Tag.Else) {
                return new If(x, s1);
            }
            Match(Tag.Else);
            s2 = Stmt();
            return new Else(x, s1, s2);
        case Tag.Id:
            return Assign();
        default:
            return Inter.Stmt.Null;
        }
    }

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
        while (look.Tag == Tag.EQ || look.Tag == Tag.NE) {
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
        case Tag.LE:
        case Tag.GE:
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
            Error($"{id} undeclared.");
        }

        if (look.Tag == Tag.Assign) {
            Move();
            stmt = new Set(id!, Factor());
        }

        Match(Tag.Semicolon);

        return stmt;
    }

    public Expr? Factor() {
        Inter.Expr? x = null;
        switch (look.Tag) {
        case Tag.Int:
            x = new Constant(look, Typ.Int);
            Move();
            return x;
        default:
            Error("Syntax error.");
            return x;
        }
    }

    public void Decls() {
        while (look.Tag == Tag.Var) {
            // TODO: Initial move can be moved to another place
            Move();

            var tok = look;
            Match(Tag.Id);
            Match(Tag.Colon);
            var p = GetTyp();
            Match(Tag.Semicolon);
            var id = new Id((Word)tok, p, used);
            top?.Add(tok, id);
            used += p.Width;
        }
    }

    public Typ GetTyp() {
        var p = look as Typ;
        if (p is null) {
            throw new InvalidCastException("Type cast failed.");
        }

        Match(Tag.Int);

        return p;
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

    public Expr Unary() {
        if (look.Tag == '-') {
            Move();
            return new Unary(Word.Minus, Unary());
        }
        else if (look.Tag == '!') {
            var tok = look;
            Move();
            return new Not(tok, Unary());
        }
        else {
            return Factor();
        }
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
}
