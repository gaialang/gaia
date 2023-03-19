namespace Gaia.AST;

public sealed class IntLiteral : Expression {
    public IntLiteral(string lexeme, int value) {
        Lexeme = lexeme;
        Value = value;
        Kind = SyntaxKind.IntLiteral;
    }

    public string Lexeme { get; private set; }
    public double Value { get; private set; }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
