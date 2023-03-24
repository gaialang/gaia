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

    public SyntaxKind Kind { get; }

    public TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
