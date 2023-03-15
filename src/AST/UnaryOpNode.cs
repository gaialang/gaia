using Gaia.Compiler;

namespace Gaia.AST;

public sealed class UnaryOpNode : Expr {
    public Token Operator { get; }
    public Node Operand { get; }
    public UnaryOpNode(Token op, Node operand) {
        Operator = op;
        Operand = operand;
        switch (op.Type) {
        case TokenType.Minus:
            NodeType = NodeType.UnaryMinus;
            break;
        case TokenType.Not:
            NodeType = NodeType.Not;
            break;
        default:
            throw new Exception("op " + op.Type + " is not a valid operator");
        }
    }

    public override NodeType NodeType { get; protected set; }

    public override TResult Accept<TResult, TContext>(Visitor<TResult, TContext> v, TContext ctx) {
        return v.Visit(this, ctx);
    }
}
