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
    private Dictionary<string, Word> words = new();

    public void Reserve(Word w) {
        words.Add(w.Lexeme, w);
    }

    public Lexer() {
        Reserve(Word.Var);
        Reserve(Word.Package);
        Reserve(Word.Func);
        Reserve(Word.Ret);

        Reserve(Typ.Int);

        source = new StreamReader(AppContext.BaseDirectory + "tests/test.ga");
    }

    public Token Scan() {
        for (; ; ReadChar()) {
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
        case '(':
            peek = ' ';
            return new Token(Tag.LParen);
        case ')':
            peek = ' ';
            return new Token(Tag.RParen);
        case '{':
            peek = ' ';
            return new Token(Tag.LBrac);
        case '}':
            peek = ' ';
            return new Token(Tag.RBrac);
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
                return new Int(v);
            }

            double x = v;
            double d = 10;

            while (true) {
                ReadChar();
                if (!char.IsDigit(peek)) {
                    break;
                }

                x += double.Parse(peek.ToString()) / 10;
                d *= 10;
            }

            return new Float64(x);
        }

        if (char.IsLetter(peek)) {
            var b = new StringBuilder();
            do {
                b.Append(peek);
                ReadChar();
            } while (char.IsLetterOrDigit(peek));

            var s = b.ToString();

            if (words.TryGetValue(s, out var value)) {
                return value;
            }

            var w = new Word(s, Tag.Id);
            words.Add(s, w);
            return w;
        }

        var tok = new Token(Tag.EOF);
        peek = ' ';
        return tok;
    }

    private void ReadChar() {
        peek = (char)source.Read();
        Pos++;
    }
}
