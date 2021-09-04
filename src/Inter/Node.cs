using System.Data;
using Gaia.Lex;

namespace Gaia.Inter {
    public class Node {
        protected static void Error(string s) {
            throw new SyntaxErrorException($"Near line {Lexer.Line} position {Lexer.Pos}: {s}.");
        }
    }
}
