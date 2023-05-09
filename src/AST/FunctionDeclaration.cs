using Gaia.Domain;

namespace Gaia.AST;

public sealed class FunctionDeclaration : Statement {
    public FunctionDeclaration(Identifier name, List<Parameter> parameters, Expression typ, Block body) {
        Name = name;
        Parameters = parameters;
        Type = typ;
        Body = body;
        Kind = SyntaxKind.FunctionDeclaration;
    }

    public Identifier Name { get; }
    public List<Parameter> Parameters { get; }

    /// <summary>
    /// Return type of the function.
    /// </summary>
    public Expression Type { get; }

    public Block Body { get; private set; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
