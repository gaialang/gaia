using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Gaia.Symbols;

namespace Gaia.Lex {
    public class Lexer {
        public static int Line { get; set; } = 1;

        private StreamReader source;
        private int pos = 0;
        private char peek = ' ';
        private Dictionary<string, Word> words = new();

        public void Reserve(Word w) {
            words.Add(w.Lexeme, w);
        }

        public Lexer() {
            Reserve(new Word("var", Tag.Var));

            Reserve(Typ.Int);

            ReadSource();
        }

        public void ReadSource() {
            source = new StreamReader(AppContext.BaseDirectory + "src/var.gaia");
        }

        public Token Scan() {
            for (;; ReadChar()) {
                if (peek == ' ' || peek == '\t') {
                    continue;
                }
                else if (peek == '\n') {
                    Line++;
                }
                else {
                    break;
                }
            }

            switch (peek) {
            case '=':
                peek = ' ';
                return new Token(Tag.Assign);
            case ':':
                peek = ' ';
                return new Token(Tag.Colon);
            case ';':
                peek = ' ';
                return new Token(Tag.Semicolon);
            default:
                break;
            }

            /*
            if (char.IsDigit(peek)) {
                int v = 0;
                do {
                    v = 10 * v + int.Parse(peek.ToString());
                    ReadChar();
                } while (char.IsDigit(peek));

                if (peek != '.') {
                    return new Num(v);
                }

                float x = v;
                float d = 10;

                for (;;) {
                    readch();
                    if (!Character.isDigit(peek)) {
                        break;
                    }
                    x = x + Character.digit(peek, 10) / 10;
                    d = d * 10;
                }

                return new Real(x);
            }
            */

            if (char.IsLetter(peek)) {
                var b = new StringBuilder();
                do {
                    b.Append(peek);
                    ReadChar();
                } while (char.IsLetterOrDigit(peek));

                var s = b.ToString();
                if (words.ContainsKey(s)) {
                    return words[s];
                }

                var w = new Word(s, Tag.Id);
                words.Add(s, w);
                return w;
            }

            var tok = new Token(Tag.Unknown);
            peek = ' ';
            return tok;
        }

        private void ReadChar() {
            peek = (char) source.Read();
        }
    }
}
