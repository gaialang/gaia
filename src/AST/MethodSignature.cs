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
    public int Pos { get; }
    public int End { get; }
    public SyntaxKind Kind { get; }

    public TResult Accept<TResult>(Visitor<TResult> visitor) {
        return visitor.Visit(this);
    }
}
