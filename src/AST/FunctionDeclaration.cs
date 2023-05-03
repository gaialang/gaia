using Gaia.Domain;

namespace Gaia.AST;

public sealed class FunctionDeclaration : Statement {
    public FunctionDeclaration(Identifier name, List<Parameter> parameters, Expression returnType, Block body) {
        Name = name;
        Parameters = parameters;
        Type = returnType;
        Body = body;
        Kind = SyntaxKind.FunctionDeclaration;
    }

    public Identifier Name { get; }
    public List<Parameter> Parameters { get; }
    public Expression Type { get; }
    public Block Body { get; private set; }

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
