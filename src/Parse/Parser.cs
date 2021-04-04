using System;
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
            Console.WriteLine("OK");
        }

        public void Move() {
            look = lexer.Scan();
        }

        public void Error(string s) {
            throw new SyntaxErrorException($"Near line {Lexer.Line} position {Lexer.Pos}: {s}.");
        }

        /// <summary>
        ///     Match and move to next token
        /// </summary>
        /// <param name="t"></param>
        public void Match(int t) {
            if (look.Tag == t) {
                Move();
            }
            else {
                Error("syntax error");
            }
        }

        public void Package() {
            if (look.Tag != Tag.Package) {
                Error("expected package");
            }

            var savedEnv = top;
            top = new Env(top);

            Match(Tag.Package);
            var tok = look;
            Match(Tag.Id);
            var id = new Id(tok as Word, Symbols.Type.Null, used);
            top?.Add(tok, id);
            used += Symbols.Type.Null.Width;
            var p = new Pkg(id);
            Match(Tag.Semicolon);
            Console.WriteLine(p.ToString());

            Decls();
            var s = Stmts();

            top = savedEnv;
        }

        public Stmt Block() {
            Match('{');
            Decls();
            var s = Stmts();
            Match('}');
            return s;
        }

        public Stmt Stmts() {
            if (look.Tag == Tag.Eof) {
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
            Match(Lex.Tag.Id);
            var id = top?.Get(t);
            if (id is null) {
                Error($"{id} undeclared");
            }

            if (look.Tag == Tag.Assign) {
                Move();
                stmt = new Set(id, Factor());
            }

            Match(Tag.Semicolon);

            return stmt;
        }

        public Expr Factor() {
            var x = Expr.Null;
            switch (look.Tag) {
            case Tag.Num:
                x = new Constant(look, Symbols.Type.Int);
                Move();
                return x;
            default:
                Error("syntax error");
                return x;
            }
        }

        public void Decls() {
            while (look.Tag == Tag.Var) {
                // TODO: Initial move can be deleted
                Move();

                var tok = look;
                Match(Tag.Id);
                Match(Tag.Colon);
                var p = Type();
                Match(Tag.Semicolon);
                var id = new Id(tok as Word, p, used);
                top?.Add(tok, id);
                used += p.Width;
            }
        }

        public Symbols.Type Type() {
            var p = look as Symbols.Type;
            if (p is null) {
                throw new InvalidCastException("Type cast failed");
            }

            Match(Tag.Basic);

            return p;
        }
    }
}
