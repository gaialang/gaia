namespace Gaia.AST;

public sealed class FunctionDeclaration : Statement {
    public FunctionDeclaration(Identifier name, List<Identifier> parameters, TypeInfo returnType, Block body) {
        Name = name;
        Parameters = parameters;
        ReturnType = returnType;
        Body = body;
        Kind = SyntaxKind.FunctionDeclaration;
    }

    public Identifier Name { get; }
    public List<Identifier> Parameters { get; }
    public TypeInfo ReturnType { get; }
    public Block Body { get; private set; }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
