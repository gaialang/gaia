using System;
using System.Data;
using Gaia.Inter;
using Gaia.Lex;
using Gaia.Symbols;

namespace Gaia.Parse {
    public class Parser {
        private Lexer lexer;
        private Token look;
        Env? top;
        int used = 0;

        public Parser(Lexer l) {
            lexer = l;
            Move();
        }

        public void Run() {
            Block();
            Console.WriteLine("OK");
        }

        public void Move() {
            look = lexer.Scan();
        }

        public void Error(string s) {
            throw new SyntaxErrorException($"Near line {Lexer.Line}: {s}");
        }

        public void Match(Tag t) {
            if (look.Tag == t) {
                Move();
            }
            else {
                Error("syntax error");
            }
        }

        public void Block() {
            // match('{');
            var savedEnv = top;
            top = new Env(top);
            Decls();
            // var s = Stmts();
            // match('}');
            top = savedEnv;
            // return s;
        }

        public Stmt Stmts() {
            return new Seq(Stmt(), Stmts());
        }

        public Stmt Stmt() {
            return new Stmt();
        }

        public void Decls() {
            while (look.Tag == Tag.Var) {
                // TODO: Initial move can be deleted
                Move();

                Match(Tag.Id);
                var tok = look;
                Match(Tag.Colon);
                var p = Type();
                Match(Tag.Semicolon);
                var id = new Id(tok as Word, p, used);
                top?.Add(tok, id);
                used += p.Width;
            }
        }

        public Typ Type() {
            var p = look as Typ;
            if (p is null) {
                throw new InvalidCastException("Type cast failed");
            }

            Match(Tag.Basic);

            return p;
        }
    }
}
