using System;
using System.Collections.Generic;
using System.Data;
using Gaia.Inter;
using Gaia.Lex;
using Gaia.Symbols;

namespace Gaia.Parse {
    public class Parser {
        private Lexer lexer;
        private Token look;
        private Env? top;
        private int used = 0;

        public Parser(Lexer l) {
            lexer = l;
            Move();
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

            Match(Tag.Int);
            Match(Tag.RParen);

            return argsList;
        }

        public Stmt Stmts() {
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
            case Tag.Id:
                return Assign();
            default:
                return Inter.Stmt.Null;
            }
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

        public Expr Factor() {
            var x = Expr.Null;
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
                var p = GetType();
                Match(Tag.Semicolon);
                var id = new Id((Word)tok, p, used);
                top?.Add(tok, id);
                used += p.Width;
            }
        }

        public Typ GetType() {
            var p = look as Typ;
            if (p is null) {
                throw new InvalidCastException("Type cast failed.");
            }

            Match(Tag.Int);

            return p;
        }
    }
}
