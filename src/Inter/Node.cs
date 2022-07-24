namespace Gaia.Inter;

using System.Data;
using Gaia.Lex;

public class Node {
    public static void Error(string s) {
        throw new SyntaxErrorException($"Near line {Lexer.Line} position {Lexer.Pos}: {s}.");
    }
}
