using System.Data;
using Gaia.Lex;

namespace Gaia.Inter {
    public class Node {
        private int lexLine = Lexer.Line;

        public void Error(string s) {
            throw new SyntaxErrorException($"Near line {lexLine} position {Lexer.Pos}: {s}");
        }
    }
}
