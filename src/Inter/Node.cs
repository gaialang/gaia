using System.Data;
using Gaia.Lex;

namespace Gaia.Inter {
    public class Node {
        private int line = Lexer.Line;
        private int pos = Lexer.Pos;

        public void Error(string s) {
            throw new SyntaxErrorException($"Near line {line} position {pos}: {s}.");
        }
    }
}
