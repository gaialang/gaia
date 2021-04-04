using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gaia.Symbols;
using Type = Gaia.Symbols.Type;

namespace Gaia.Lex {
    public class Lexer {
        public static int Line { get; private set; } = 1;
        public static int Pos { get; private set; } = 0;

        private StreamReader source;
        private char peek = ' ';
        private Dictionary<string, Word> words = new();

        public void Reserve(Word w) {
            words.Add(w.Lexeme, w);
        }

        public Lexer() {
            Reserve(new Word("var", Tag.Var));

            Reserve(Type.Int);

            source = new StreamReader(AppContext.BaseDirectory + "src/test.gaia");
        }

        public Token Scan() {
            for (;; ReadChar()) {
                if (peek == ' ' || peek == '\t') {
                    continue;
                }

                if (peek == '\n') {
                    Line++;
                    Pos = 0;
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

            if (char.IsDigit(peek)) {
                var v = 0;
                do {
                    v = 10 * v + int.Parse(peek.ToString());
                    ReadChar();
                } while (char.IsDigit(peek));

                if (peek != '.') {
                    return new Num(v);
                }

                float x = v;
                float d = 10;

                while (true) {
                    ReadChar();
                    if (!char.IsDigit(peek)) {
                        break;
                    }

                    x += float.Parse(peek.ToString()) / 10;
                    d *= 10;
                }

                return new Real(x);
            }

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

            var tok = new Token(Tag.Eof);
            peek = ' ';
            return tok;
        }

        private void ReadChar() {
            peek = (char) source.Read();
            Pos++;
        }
    }
}
