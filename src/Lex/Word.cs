﻿namespace Gaia.Lex;

public class Word : Token {
    public readonly string Lexeme;

    public static readonly Word Var = new("var", Lex.Tag.Var);
    public static readonly Word Package = new("package", Lex.Tag.Pkg);
    public static readonly Word Func = new("func", Lex.Tag.Func);
    public static readonly Word Ret = new("return", Lex.Tag.Ret);
    public static readonly Word Minus = new("minus", Lex.Tag.Minus);

    public Word(string s, int tag) : base(tag) {
        Lexeme = s;
    }

    public override string ToString() {
        return Lexeme;
    }
}
