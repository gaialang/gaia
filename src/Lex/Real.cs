namespace Gaia.Lex {
    public class Real : Token {
        public readonly float Value;

        public Real(float v) : base(Lex.Tag.Num) {
            Value = v;
        }

        public new string ToString() {
            return Value.ToString();
        }
    }
}
