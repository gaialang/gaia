namespace Gaia.AST;

public sealed class PackageNode : Node {
    public readonly string Name;
    public readonly List<Node> ExprList;
    public override NodeType NodeType { get; protected set; }

    public PackageNode(string name, List<Node> exprList) {
        Name = name;
        ExprList = exprList;
        NodeType = NodeType.Package;
    }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
