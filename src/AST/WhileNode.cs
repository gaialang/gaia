namespace Gaia.AST;

public sealed class WhileNode : Stmt {
    public WhileNode(string name, List<IdNode> args, IdType returnType) {
        Name = name;
        Arguments = args;
        ReturnType = returnType;
        NodeType = NodeType.While;
    }

    public string Name { get; }
    public List<IdNode> Arguments { get; }
    public IdType ReturnType { get; }
    public override NodeType NodeType { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext ctx) {
        return visitor.Visit(this, ctx);
    }
}
