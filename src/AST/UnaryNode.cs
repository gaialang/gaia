using Gaia.Compiler;

namespace Gaia.AST;

public sealed class UnaryNode : Expr {
    public Token Operator { get; }
    public Node Operand { get; }
    public UnaryNode(Token op, Node operand) {
        Operator = op;
        Operand = operand;
        NodeType = NodeType.Unary;
    }

    public override NodeType NodeType { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
