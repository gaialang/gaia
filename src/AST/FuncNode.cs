namespace Gaia.AST;

public sealed class FuncNode : Stmt {
    public FuncNode(string name, List<IdNode> args, IdType returnType, List<Node> body) {
        Name = name;
        Arguments = args;
        ReturnType = returnType;
        Body = body;
        NodeType = NodeType.Func;
    }

    public string Name { get; }
    public List<IdNode> Arguments { get; }
    public IdType ReturnType { get; }
    public List<Node> Body { get; private set; }

    public override NodeType NodeType { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
