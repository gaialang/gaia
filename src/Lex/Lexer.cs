namespace Gaia.Lex;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gaia.Symbols;

public class Lexer {
    public static int Line { get; private set; } = 1;
    public static int Pos { get; private set; } = 0;

    private StreamReader source;
    private char peek = ' ';
    private bool ended = false;
    private Dictionary<string, Word> words = new();

    public Lexer() {
        Reserve(new Word("var", Tag.Var));
        Reserve(new Word("return", Tag.Ret));
        Reserve(new Word("func", Tag.Func));
        Reserve(new Word("if", Tag.If));
        Reserve(new Word("else", Tag.Else));
        Reserve(new Word("while", Tag.While));
        Reserve(new Word("break", Tag.Break));
        Reserve(new Word("loop", Tag.Loop));
        Reserve(new Word("import", Tag.Import));

        Reserve(Word.True);
        Reserve(Word.False);

        Reserve(Typing.Int);
        Reserve(Typing.Char);
        Reserve(Typing.Bool);
        Reserve(Typing.Nil);
        Reserve(Typing.Pkg);

        source = new StreamReader(AppContext.BaseDirectory + "tests/test.ga");
    }

    public void Reserve(Word w) {
        words.Add(w.Lexeme, w);
    }

    public Token Scan() {
        for (; ; Readch()) {
            if (ended) {
                var t = new Token(Tag.Eof);
                peek = ' ';
                return t;
            }

            if (peek == ' ' || peek == '\t' || peek == '\r') {
                continue;
            } else if (peek == '\n') {
                Line++;
                Pos = 0;
            } else {
                break;
            }
        }

        switch (peek) {
        case '&':
            if (Readch('&')) {
                return Word.And;
            } else {
                return new Token('&');
            }
        case '|':
            if (Readch('|')) {
                return Word.Or;
            } else {
                return new Token('|');
            }
        case '=':
            if (Readch('=')) {
                return Word.Eq;
            } else {
                return new Token('=');
            }
        case '!':
            if (Readch('=')) {
                return Word.Ne;
            } else {
                return new Token('!');
            }
        case '<':
            if (Readch('=')) {
                return Word.Le;
            } else {
                return new Token('<');
            }
        case '>':
            if (Readch('=')) {
                return Word.Ge;
            } else {
                return new Token('>');
            }
        default:
            break;
        }

        if (char.IsDigit(peek)) {
            var v = 0;
            do {
                v = 10 * v + int.Parse(peek.ToString());
                Readch();
            } while (char.IsDigit(peek));

            if (peek != '.') {
                return new Num(v);
            }

            double x = v;
            double d = 10;

            for (; ; ) {
                Readch();
                if (!char.IsDigit(peek)) {
                    break;
                }
                x += double.Parse(peek.ToString()) / d;
                d *= 10;
            }

            return new Real(x);
        }

        if (char.IsLetter(peek)) {
            var b = new StringBuilder();
            do {
                b.Append(peek);
                Readch();
            } while (char.IsLetterOrDigit(peek));

            var s = b.ToString();

            if (words.TryGetValue(s, out var value)) {
                return value;
            }

            var w = new Word(s, Tag.Id);
            words.Add(s, w);
            return w;
        }

        var tok = new Token(peek);
        peek = ' ';
        return tok;
    }

    // Read a char.
    private void Readch() {
        var n = source.Read();
        if (n == -1) {
            ended = true;
            return;
        }
        peek = (char)n;
        Pos++;
    }

    private bool Readch(char c) {
        Readch();
        if (peek != c) {
            return false;
        }
        peek = ' ';
        return true;
    }
}
