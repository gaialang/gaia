namespace Gaia.Lex {
    public class Num : Token {
        public readonly int Value;

        public Num(int v) : base(Lex.Tag.Num) {
            Value = v;
        }

        public new string ToString() {
            return Value.ToString();
        }
    }
}
