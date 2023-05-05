using Gaia.Domain;

namespace Gaia.AST;

public sealed class Identifier : Expression {
    public Identifier(string text) {
        Text = text;
        Kind = SyntaxKind.Identifier;
    }

    public string Text { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
