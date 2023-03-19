namespace Gaia.AST;

public sealed class IntLiteral : Expr {
    public IntLiteral(string lexeme, int value) {
        Lexeme = lexeme;
        Value = value;
    }

    public string Lexeme { get; private set; }
    public double Value { get; private set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
