using Gaia.Domain;

namespace Gaia.AST;

public sealed class Identifier : Expression {
    public Identifier(string name) {
        Name = name;
        Kind = SyntaxKind.Identifier;
    }

    /// <summary>
    /// Maybe escapedText?
    /// </summary>
    public string Name { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
