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
        } else {
            Error($"token {look} != {t}");
        }
    }

    public void Package() {
        if (look.Tag != Tag.Pkg) {
            Error("Expected package");
        }

        var savedEnv = top;
        top = new Env(top);

        // Match package clause.
        Match(Tag.Pkg);
        var tok = look;
        Match(Tag.Id);
        var id = new Id(tok, Typing.Pkg, used);
        top?.Add(tok, id);
        used += Typing.Pkg.Width;
        Match(';');

        VarDecls();
        var s = Stmts();
        var f = FuncDecl();

        var p = new Pkg(id);
        top = savedEnv;
    }

    public Stmt Block() {
        Match('{');

        VarDecls();
        var s = Stmts();
        Match(Tag.Ret);
        var t = look;
        Match(Tag.Id);

        var id = top?.Get(t);
        if (id is null) {
            Error($"{id} undeclared");
        }

        Match(';');
        Match('}');
        return s;
    }

    // TODO:
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
        var argsList = new List<Id>();

        var tok = look;
        Match(Tag.Id);
        Match(':');
        var p = GetTyping();

        var id = new Id(tok, p, used);
        top?.Add(tok, id);
        used += Typing.Int.Width;
        argsList.Add(id);

        return argsList;
    }

    public Stmt Stmts() {
        switch (look.Tag) {
        case Tag.Func:
        case Tag.Ret:
        case '}':
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
            s1 = Stmt();
            if (look.Tag != Tag.Else) {
                return new If(x, s1);
            }
            Match(Tag.Else);
            s2 = Stmt();
            return new Else(x, s1, s2);
        // case Tag.While:
        //         var whilenode = new While();
        //         savedStmt = Stmt.Enclosing;
        //         Stmt.Enclosing = whilenode;
        //         match(Tag.WHILE);
        //         match('(');
        //         x = bool();
        //         match(')');
        //         s1 = stmt();
        //         whilenode.init(x, s1);
        //         Stmt.Enclosing = savedStmt;
        //         return whilenode;
        //     case Tag.DO:
        //         var donode = new Do();
        //         savedStmt = Stmt.Enclosing;
        //         Stmt.Enclosing = donode;
        //         match(Tag.DO);
        //         s1 = stmt();
        //         match(Tag.WHILE);
        //         match('(');
        //         x = bool();
        //         match(')');
        //         match(';');
        //         donode.init(s1, x);
        //         Stmt.Enclosing = savedStmt;
        //         return donode;
        // case Tag.BREAK:
        //         match(Tag.BREAK);
        //         match(';');
        //         return new Break();
        case '{':
            return Block();
        default:
            return Assign();
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
        }

        Match(';');
        return stmt;
    }

    public Expr Factor() {
        Inter.Expr x;
        switch (look.Tag) {
        case Tag.Num:
            x = new Constant(look, Typing.Int);
            Move();
            return x;
        default:
            Error("Syntax error");
            return null;
        }
    }

    public void VarDecls() {
        // TODO: Initial move can be moved to another place
        while (look.Tag == Tag.Var) {
            Move();
            var tok = look;
            Match(Tag.Id);
            Match(':');
            var p = GetTyping();
            Match(';');
            var id = new Id(tok, p, used);
            top?.Add(tok, id);
            used += p.Width;
        }
    }

    // Get a type for the variable.
    public Typing GetTyping() {
        var p = look as Typing;
        if (p is null) {
            throw new InvalidCastException("Type cast failed.");
        }

        Match(Tag.Basic);
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
        } else if (look.Tag == '!') {
            var tok = look;
            Move();
            return new Not(tok, Unary());
        } else {
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
