using Gaia.Domain;

namespace Gaia.AST;

public sealed class MethodSignature : Statement {
    public MethodSignature(Identifier name, List<Parameter> parameters, Expression typ) {
        Name = name;
        Parameters = parameters;
        Type = typ;
        Kind = SyntaxKind.MethodSignature;
    }

    public Identifier Name { get; }
    public List<Parameter> Parameters { get; }

    /// <summary>
    /// Return type.
    /// </summary>
    public Expression Type { get; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
