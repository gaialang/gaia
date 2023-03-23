namespace Gaia.AST;

public sealed class BoolLiteral : Expression {
    public BoolLiteral(string lexeme) {
        Lexeme = lexeme;
        Kind = SyntaxKind.BoolLiteral;
    }

    public string Lexeme { get; private set; }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
