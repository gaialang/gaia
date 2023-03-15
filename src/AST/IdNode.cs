namespace Gaia.AST;

public sealed class IdNode : Node {
    public IdNode(string name, IdType t) {
        Name = name;
        IdType = t;
        NodeType = NodeType.Id;
    }

    public string Name { get; }
    public IdType IdType { get; }
    public override NodeType NodeType { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
