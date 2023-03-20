namespace Gaia.AST;

public sealed class FunctionDeclaration : Statement {
    public FunctionDeclaration(string name, List<Identifier> parameters, IdType returnType, Block body) {
        Name = name;
        Parameters = parameters;
        ReturnType = returnType;
        Body = body;
        Kind = SyntaxKind.FunctionDeclaration;
    }

    public string Name { get; }
    public List<Identifier> Parameters { get; }
    public IdType ReturnType { get; }
    public Block Body { get; private set; }

    public override SyntaxKind Kind { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
